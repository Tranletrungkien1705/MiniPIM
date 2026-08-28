import React, { useEffect, useState } from 'react'
import { Routes, Route, NavLink, Outlet } from 'react-router-dom'
import { api, fmtMoney, fmtDate } from './api'

function Flash({ msg }) { return msg ? <div className={`flash ${msg.ok ? 'ok' : 'err'}`}>{msg.text}</div> : null }
function Modal({ title, onClose, wide, children }) {
  return (
    <div className="modal-bg" onClick={onClose}>
      <div className="modal" style={wide ? { maxWidth: 720 } : undefined} onClick={e => e.stopPropagation()}>
        <div className="row" style={{ marginBottom: 12 }}><h2 style={{ flex: 1, margin: 0 }}>{title}</h2>
          <button className="btn gray sm" style={{ flex: 'none' }} onClick={onClose}>Đóng</button></div>
        {children}
      </div>
    </div>
  )
}
function Field({ label, children }) { return <div style={{ flex: 1 }}><label>{label}</label>{children}</div> }

function Layout() {
  return (
    <>
      <nav className="nav">
        <span className="brand">🏷️ MiniPIM</span>
        <NavLink to="/" end>Tổng quan</NavLink>
        <NavLink to="/products">Sản phẩm</NavLink>
        <NavLink to="/groups">Nhóm</NavLink>
      </nav>
      <div className="wrap"><Outlet /></div>
    </>
  )
}

function Dashboard() {
  const [d, setD] = useState(null); const [cache, setCache] = useState('')
  useEffect(() => { api.dashboard().then(r => { setD(r.data); setCache(r.cache) }) }, [])
  if (!d) return <p className="muted">Đang tải…</p>
  const max = Math.max(1, ...d.byGroup.map(g => g.count))
  return (
    <>
      <h1>Tổng quan PIM {cache && <span className="pill">cache: {cache}</span>}</h1>
      <div className="grid kpis" style={{ marginBottom: 18 }}>
        <div className="kpi"><div className="v">{d.products}</div><div className="l">Sản phẩm</div></div>
        <div className="kpi"><div className="v">{d.active}</div><div className="l">Đang bán</div></div>
        <div className="kpi"><div className="v">{d.groups}</div><div className="l">Nhóm hàng</div></div>
        <div className="kpi"><div className="v">{d.withBom}</div><div className="l">Có định mức (BOM)</div></div>
      </div>
      <div className="card funnel">
        <h2>Sản phẩm theo nhóm</h2>
        {d.byGroup.map((g, i) => (
          <div className="bar" key={i}><div className="lbl">{g.group}</div>
            <div className="track"><div className="fill" style={{ width: `${(g.count / max) * 100}%` }} /></div>
            <div className="n">{g.count}</div></div>
        ))}
      </div>
    </>
  )
}

function Products() {
  const [rows, setRows] = useState([]); const [q, setQ] = useState(''); const [gid, setGid] = useState('')
  const [groups, setGroups] = useState([]); const [open, setOpen] = useState(null); const [creating, setCreating] = useState(false)
  const load = () => api.products(q, gid || null).then(r => setRows(r.data))
  useEffect(() => { load() }, [gid])
  useEffect(() => { api.groups().then(r => setGroups(r.data)) }, [])
  return (
    <>
      <div className="toolbar"><h1 style={{ margin: 0, flex: 'none' }}>Sản phẩm</h1><div className="sp" />
        <select style={{ maxWidth: 180 }} value={gid} onChange={e => setGid(e.target.value)}><option value="">— Nhóm —</option>{groups.map(g => <option key={g.id} value={g.id}>{g.name}</option>)}</select>
        <input style={{ maxWidth: 220 }} placeholder="Tìm mã/tên/barcode…" value={q} onChange={e => setQ(e.target.value)} onKeyDown={e => e.key === 'Enter' && load()} />
        <button className="btn ghost sm" style={{ flex: 'none' }} onClick={load}>Tìm</button>
        <button className="btn sm" style={{ flex: 'none' }} onClick={() => setCreating(true)}>+ Thêm</button></div>
      <div className="card" style={{ padding: 0, overflow: 'auto' }}>
        <table>
          <thead><tr><th>SKU</th><th>Tên</th><th>Nhóm</th><th>ĐVT</th><th className="right">Giá vốn</th><th className="right">Giá bán</th><th>Trạng thái</th></tr></thead>
          <tbody>{rows.map(p => (
            <tr key={p.id} style={{ cursor: 'pointer' }} onClick={() => setOpen(p.id)}>
              <td>{p.code}</td><td>{p.name}</td><td>{p.group || '—'}</td><td>{p.uom}</td>
              <td className="right">{fmtMoney(p.costPrice)}</td><td className="right">{fmtMoney(p.salePrice)}</td>
              <td><span className={`badge ${p.status === 0 ? 'success' : 'dark'}`}>{p.statusText}</span></td></tr>))}
            {rows.length === 0 && <tr><td colSpan={7} className="muted" style={{ padding: 20 }}>Không có sản phẩm.</td></tr>}
          </tbody>
        </table>
      </div>
      {open && <ProductDetail id={open} groups={groups} onClose={() => setOpen(null)} onSaved={() => { setOpen(null); load() }} />}
      {creating && <ProductDetail id={0} groups={groups} onClose={() => setCreating(false)} onSaved={() => { setCreating(false); load() }} />}
    </>
  )
}

function ProductDetail({ id, groups, onClose, onSaved }) {
  const isNew = id === 0
  const [p, setP] = useState(isNew ? { id: 0, code: '', name: '', groupId: '', uom: 'cái', barcode: '', costPrice: 0, salePrice: 0, description: '', status: 0, attributes: [], bom: [] } : null)
  const [edit, setEdit] = useState(isNew)
  const [msg, setMsg] = useState(null); const [err, setErr] = useState('')
  useEffect(() => { if (!isNew) api.product(id).then(r => setP({ ...r.data, groupId: r.data.groupId ?? '', attributes: r.data.attributes || [], bom: r.data.bom || [] })) }, [id])
  if (!p) return <Modal title="…" onClose={onClose}><p className="muted">Đang tải…</p></Modal>
  const up = (k, v) => setP({ ...p, [k]: v })
  const setAttr = (i, k, v) => up('attributes', p.attributes.map((a, j) => j === i ? { ...a, [k]: v } : a))
  const setBom = (i, k, v) => up('bom', p.bom.map((b, j) => j === i ? { ...b, [k]: v } : b))
  const save = async () => {
    try {
      await api.saveProduct({
        id: p.id, code: p.code, name: p.name, groupId: p.groupId ? Number(p.groupId) : null, uom: p.uom, barcode: p.barcode,
        costPrice: Number(p.costPrice), salePrice: Number(p.salePrice), description: p.description, status: Number(p.status),
        attributes: p.attributes.filter(a => a.name), bom: p.bom.filter(b => b.componentName)
      })
      onSaved()
    } catch (e) { setErr(e.message) }
  }
  return (
    <Modal title={isNew ? 'Thêm sản phẩm' : `${p.code} — ${p.name}`} onClose={onClose} wide>
      <Flash msg={msg} />{err && <Flash msg={{ ok: false, text: err }} />}
      {!edit ? (
        <>
          <dl className="dl">
            <dt>Nhóm</dt><dd>{p.group || '—'}</dd><dt>ĐVT</dt><dd>{p.uom}</dd>
            <dt>Barcode</dt><dd>{p.barcode || '—'}</dd>
            <dt>Giá vốn / bán</dt><dd>{fmtMoney(p.costPrice)} / {fmtMoney(p.salePrice)}</dd>
            <dt>Mô tả</dt><dd>{p.description || '—'}</dd>
          </dl>
          {p.attributes.length > 0 && <><div className="section-t">Thuộc tính</div><table><tbody>{p.attributes.map((a, i) => <tr key={i}><td className="muted">{a.name}</td><td>{a.value}</td></tr>)}</tbody></table></>}
          {p.bom.length > 0 && <><div className="section-t">Định mức (BOM)</div><table><thead><tr><th>Mã</th><th>Thành phần</th><th className="right">SL</th><th>ĐVT</th></tr></thead><tbody>{p.bom.map((b, i) => <tr key={i}><td>{b.componentCode}</td><td>{b.componentName}</td><td className="right">{b.quantity}</td><td>{b.uom}</td></tr>)}</tbody></table></>}
          <div style={{ marginTop: 16 }}><button className="btn" onClick={() => setEdit(true)}>Sửa</button></div>
        </>
      ) : (
        <>
          <div className="row"><Field label="Tên *"><input value={p.name} onChange={e => up('name', e.target.value)} /></Field>
            <Field label="SKU"><input value={p.code} onChange={e => up('code', e.target.value)} disabled={!isNew} /></Field></div>
          <div className="row"><Field label="Nhóm"><select value={p.groupId} onChange={e => up('groupId', e.target.value)}><option value="">—</option>{groups.map(g => <option key={g.id} value={g.id}>{g.name}</option>)}</select></Field>
            <Field label="ĐVT"><input value={p.uom} onChange={e => up('uom', e.target.value)} /></Field>
            <Field label="Barcode"><input value={p.barcode} onChange={e => up('barcode', e.target.value)} /></Field></div>
          <div className="row"><Field label="Giá vốn"><input type="number" value={p.costPrice} onChange={e => up('costPrice', e.target.value)} /></Field>
            <Field label="Giá bán"><input type="number" value={p.salePrice} onChange={e => up('salePrice', e.target.value)} /></Field>
            <Field label="Trạng thái"><select value={p.status} onChange={e => up('status', e.target.value)}><option value={0}>Đang bán</option><option value={1}>Ngừng</option></select></Field></div>
          <Field label="Mô tả"><input value={p.description} onChange={e => up('description', e.target.value)} /></Field>
          <div className="section-t">Thuộc tính động</div>
          {p.attributes.map((a, i) => (
            <div className="row" key={i} style={{ marginBottom: 6 }}>
              <input placeholder="Tên" value={a.name} onChange={e => setAttr(i, 'name', e.target.value)} />
              <input placeholder="Giá trị" value={a.value || ''} onChange={e => setAttr(i, 'value', e.target.value)} />
              <button className="btn gray sm" style={{ flex: 'none' }} onClick={() => up('attributes', p.attributes.filter((_, j) => j !== i))}>×</button>
            </div>))}
          <button className="btn ghost sm" onClick={() => up('attributes', [...p.attributes, { name: '', value: '' }])}>+ Thuộc tính</button>
          <div className="section-t">Định mức (BOM)</div>
          {p.bom.map((b, i) => (
            <div className="row" key={i} style={{ marginBottom: 6 }}>
              <input placeholder="Mã" value={b.componentCode || ''} onChange={e => setBom(i, 'componentCode', e.target.value)} />
              <input placeholder="Thành phần" value={b.componentName} onChange={e => setBom(i, 'componentName', e.target.value)} />
              <input placeholder="SL" type="number" style={{ maxWidth: 80 }} value={b.quantity} onChange={e => setBom(i, 'quantity', Number(e.target.value))} />
              <button className="btn gray sm" style={{ flex: 'none' }} onClick={() => up('bom', p.bom.filter((_, j) => j !== i))}>×</button>
            </div>))}
          <button className="btn ghost sm" onClick={() => up('bom', [...p.bom, { componentCode: '', componentName: '', quantity: 1, uom: 'cái' }])}>+ Thành phần</button>
          <div style={{ marginTop: 16 }}><button className="btn" onClick={save}>Lưu</button></div>
        </>
      )}
    </Modal>
  )
}

function Groups() {
  const [rows, setRows] = useState([]); const [name, setName] = useState(''); const [code, setCode] = useState(''); const [err, setErr] = useState('')
  const load = () => api.groups().then(r => setRows(r.data))
  useEffect(() => { load() }, [])
  const add = async () => { try { if (!name) return; await api.createGroup({ name, code }); setName(''); setCode(''); load() } catch (e) { setErr(e.message) } }
  return (
    <>
      <h1>Nhóm hàng</h1>
      {err && <Flash msg={{ ok: false, text: err }} />}
      <div className="card"><div className="row">
        <Field label="Mã"><input value={code} onChange={e => setCode(e.target.value)} /></Field>
        <Field label="Tên nhóm"><input value={name} onChange={e => setName(e.target.value)} /></Field>
        <div style={{ flex: 'none', alignSelf: 'flex-end' }}><button className="btn" onClick={add}>+ Thêm</button></div>
      </div></div>
      <div className="card" style={{ padding: 0, overflow: 'auto' }}>
        <table><thead><tr><th>Mã</th><th>Tên</th></tr></thead>
          <tbody>{rows.map(g => <tr key={g.id}><td>{g.code}</td><td>{g.name}</td></tr>)}</tbody></table>
      </div>
    </>
  )
}

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<Layout />}>
        <Route index element={<Dashboard />} />
        <Route path="products" element={<Products />} />
        <Route path="groups" element={<Groups />} />
      </Route>
    </Routes>
  )
}
