const base = '/api/v1'
async function req(path, opts = {}) {
  const res = await fetch(base + path, {
    headers: { 'Content-Type': 'application/json' }, credentials: 'same-origin',
    ...opts, body: opts.body ? JSON.stringify(opts.body) : undefined
  })
  const text = await res.text(); const data = text ? JSON.parse(text) : null
  if (!res.ok) throw new Error(data?.error || `Lỗi ${res.status}`)
  return { data, cache: res.headers.get('X-Cache') }
}
export const api = {
  dashboard: () => req('/dashboard'),
  groups: () => req('/groups'),
  createGroup: (b) => req('/groups', { method: 'POST', body: b }),
  products: (q, groupId) => req(`/products?${q ? `q=${encodeURIComponent(q)}&` : ''}${groupId ? `groupId=${groupId}` : ''}`),
  product: (id) => req(`/products/${id}`),
  saveProduct: (b) => req('/products', { method: 'POST', body: b })
}
export const fmtMoney = (n) => (n ?? 0).toLocaleString('vi-VN') + ' ₫'
export const fmtDate = (s) => s ? new Date(s).toLocaleDateString('vi-VN') : '—'
