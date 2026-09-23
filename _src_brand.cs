using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Http;

using CmUtils = CommonUtils;
using TUtils = ProductCenter.Utils;
using TJson = Newtonsoft.Json;

using ProductCenter.Common.Models;

namespace ProductCenter.Biz.Web.Controllers
{
    public class MstBrandController : ApiControllerBase
    {
        [AcceptVerbs("POST")]
        public ServiceResult<RT_Mst_Brand> WA_Mst_Brand_Get(RQ_Mst_Brand objRQ_Mst_Brand)
        {
            #region // Temp:
            DataSet mdsReturn = CmUtils.CMyDataSet.NewMyDataSet(objRQ_Mst_Brand.Tid);
            RT_Mst_Brand objRT_Mst_Brand = null;
            DateTime dtimeSys = DateTime.Now;
            string strControllerName = "WA_Mst_Brand_Get";
            string strErrorCodeDefault = "WA_Mst_Brand_Get";

            ArrayList alParamsCoupleError = new ArrayList();
            alParamsCoupleError.AddRange(new object[] {
                "strControllerName", strControllerName
                , "dtimeSys", dtimeSys.ToString("yyyy-MM-dd HH:mm:ss")
				//, "_biz._cf.nvcParams", TJson.JsonConvert.SerializeObject(_biz._cf.nvcParams)
				//, "_biz.RQ_Mst_Brand", TJson.JsonConvert.SerializeObject(objRQ_Mst_Brand)
				////
				});
            #endregion

            try
            {
                #region // CheckGatewayAuthentication:
                TUtils.CConnectionManager.CheckGatewayAuthentication(
                    _biz._cf.nvcParams // nvcParams
                    , ref alParamsCoupleError // alParamsCoupleError
                    , objRQ_Mst_Brand.GwUserCode // strGwUserCode
                    , objRQ_Mst_Brand.GwPassword // strGwPassword
                    );
                #endregion

                #region // Process:
                //
                mdsReturn = _biz.WAS_Mst_Brand_Get(
                    ref alParamsCoupleError // alParamsCoupleError
                    , objRQ_Mst_Brand // objRQ_Mst_Brand
                                     // //
                    , out objRT_Mst_Brand // RT_Mst_Brand
                    );

                if (CmUtils.CMyDataSet.HasError(mdsReturn))
                {
                    throw CmUtils.CMyException.Raise(
                        (string)CmUtils.CMyDataSet.GetErrorCode(mdsReturn)
                        , null
                        , null
                        );
                }
                #endregion

                // Return Good:
                objRT_Mst_Brand.c_K_DT_Sys = TUtils.CMyDataList.CProcessMyDS(mdsReturn);
                return Success<RT_Mst_Brand>(objRT_Mst_Brand);
            }
            catch (Exception ex)
            {
                #region // Catch of try:
                ////
                TUtils.CProcessExc.Process(
                    ref mdsReturn // mdsFinal
                    , ex // exc
                    , strErrorCodeDefault // strErrorCode
                    , alParamsCoupleError.ToArray() // arrobjErrorParams
                    );

                // Return Bad:
                if (objRT_Mst_Brand == null) objRT_Mst_Brand = new RT_Mst_Brand();
                objRT_Mst_Brand.c_K_DT_Sys = TUtils.CMyDataList.CProcessMyDS(mdsReturn);
                return Error<RT_Mst_Brand>(ex, objRT_Mst_Brand);
                #endregion
            }
        }

        [AcceptVerbs("POST")]
        public ServiceResult<RT_Mst_Brand> WA_Mst_Brand_Create(RQ_Mst_Brand objRQ_Mst_Brand)
        {
            #region // Temp:
            DataSet mdsReturn = CmUtils.CMyDataSet.NewMyDataSet(objRQ_Mst_Brand.Tid);
            RT_Mst_Brand objRT_Mst_Brand = null;
            DateTime dtimeSys = DateTime.Now;
            string strControllerName = "WA_Mst_Brand_Creat";
            string strErrorCodeDefault = "WA_Mst_Brand_Creat";

            ArrayList alParamsCoupleError = new ArrayList();
            alParamsCoupleError.AddRange(new object[] {
                "strControllerName", strControllerName
                , "dtimeSys", dtimeSys.ToString("yyyy-MM-dd HH:mm:ss")
				////
				//, "_biz._cf.nvcParams", TJson.JsonConvert.SerializeObject(_biz._cf.nvcParams)
				//, "_biz.RQ_Mst_Brand", TJson.JsonConvert.SerializeObject(objRQ_Mst_Brand)
				});
            #endregion

            try
            {
                #region // CheckGatewayAuthentication:
                TUtils.CConnectionManager.CheckGatewayAuthentication(
                    _biz._cf.nvcParams // nvcParams
                    , ref alParamsCoupleError // alParamsCoupleError
                    , objRQ_Mst_Brand.GwUserCode // strGwUserCode
                    , objRQ_Mst_Brand.GwPassword // strGwPassword
                    );
                #endregion

                #region // Process:
                //
                mdsReturn = _biz.WAS_Mst_Brand_Create_Sync(
                    ref alParamsCoupleError // alParamsCoupleError
                    , objRQ_Mst_Brand // objRQ_Mst_Brand
                                     // //
                    , out objRT_Mst_Brand // RT_Mst_Brand
                    );

                if (CmUtils.CMyDataSet.HasError(mdsReturn))
                {
                    throw CmUtils.CMyException.Raise(
                        (string)CmUtils.CMyDataSet.GetErrorCode(mdsReturn)
                        , null
                        , null
                        );
                }
                #endregion

                // Return Good:
                objRT_Mst_Brand.c_K_DT_Sys = TUtils.CMyDataList.CProcessMyDS(mdsReturn);
                return Success<RT_Mst_Brand>(objRT_Mst_Brand);
            }
            catch (Exception ex)
            {
                #region // Catch of try:
                ////
                TUtils.CProcessExc.Process(
                    ref mdsReturn // mdsFinal
                    , ex // exc
                    , strErrorCodeDefault // strErrorCode
                    , alParamsCoupleError.ToArray() // arrobjErrorParams
                    );

                // Return Bad:
                if (objRT_Mst_Brand == null) objRT_Mst_Brand = new RT_Mst_Brand();
                objRT_Mst_Brand.c_K_DT_Sys = TUtils.CMyDataList.CProcessMyDS(mdsReturn);
                return Error<RT_Mst_Brand>(ex, objRT_Mst_Brand);
                #endregion
            }
        }

        [AcceptVerbs("POST")]
        public ServiceResult<RT_Mst_Brand> WA_Mst_Brand_Update(RQ_Mst_Brand objRQ_Mst_Brand)
        {
            #region // Temp:
            DataSet mdsReturn = CmUtils.CMyDataSet.NewMyDataSet(objRQ_Mst_Brand.Tid);
            RT_Mst_Brand objRT_Mst_Brand = null;
            DateTime dtimeSys = DateTime.Now;
            string strControllerName = "WA_Mst_Brand_Update";
            string strErrorCodeDefault = "WA_Mst_Brand_Update";

            ArrayList alParamsCoupleError = new ArrayList();
            alParamsCoupleError.AddRange(new object[] {
                "strControllerName", strControllerName
                , "dtimeSys", dtimeSys.ToString("yyyy-MM-dd HH:mm:ss")
				//, "_biz._cf.nvcParams", TJson.JsonConvert.SerializeObject(_biz._cf.nvcParams)
				//, "_biz.RQ_Mst_Brand", TJson.JsonConvert.SerializeObject(objRQ_Mst_Brand)
				////
				});
            #endregion

            try
            {
                #region // CheckGatewayAuthentication:
                TUtils.CConnectionManager.CheckGatewayAuthentication(
                    _biz._cf.nvcParams // nvcParams
                    , ref alParamsCoupleError // alParamsCoupleError
                    , objRQ_Mst_Brand.GwUserCode // strGwUserCode
                    , objRQ_Mst_Brand.GwPassword // strGwPassword
                    );
                #endregion

                #region // Process:
                //
                mdsReturn = _biz.WAS_Mst_Brand_Update_Sync(
                    ref alParamsCoupleError // alParamsCoupleError
                    , objRQ_Mst_Brand // objRQ_Mst_Brand
                                     // //
                    , out objRT_Mst_Brand // RT_Mst_Brand
                    );

                if (CmUtils.CMyDataSet.HasError(mdsReturn))
                {
                    throw CmUtils.CMyException.Raise(
                        (string)CmUtils.CMyDataSet.GetErrorCode(mdsReturn)
                        , null
                        , null
                        );
                }
                #endregion

                // Return Good:
                objRT_Mst_Brand.c_K_DT_Sys = TUtils.CMyDataList.CProcessMyDS(mdsReturn);
                return Success<RT_Mst_Brand>(objRT_Mst_Brand);
            }
            catch (Exception ex)
            {
                #region // Catch of try:
                ////
                TUtils.CProcessExc.Process(
                    ref mdsReturn // mdsFinal
                    , ex // exc
                    , strErrorCodeDefault // strErrorCode
                    , alParamsCoupleError.ToArray() // arrobjErrorParams
                    );

                // Return Bad:
                if (objRT_Mst_Brand == null) objRT_Mst_Brand = new RT_Mst_Brand();
                objRT_Mst_Brand.c_K_DT_Sys = TUtils.CMyDataList.CProcessMyDS(mdsReturn);
                return Error<RT_Mst_Brand>(ex, objRT_Mst_Brand);
                #endregion
            }
        }

        [AcceptVerbs("POST")]
        public ServiceResult<RT_Mst_Brand> WA_Mst_Brand_Delete(RQ_Mst_Brand objRQ_Mst_Brand)
        {
            #region // Temp:
            DataSet mdsReturn = CmUtils.CMyDataSet.NewMyDataSet(objRQ_Mst_Brand.Tid);
            RT_Mst_Brand objRT_Mst_Brand = null;
            DateTime dtimeSys = DateTime.Now;
            string strControllerName = "WA_Mst_Brand_Delete";
            string strErrorCodeDefault = "WA_Mst_Brand_Delete";

            ArrayList alParamsCoupleError = new ArrayList();
            alParamsCoupleError.AddRange(new object[] {
                "strControllerName", strControllerName
                , "dtimeSys", dtimeSys.ToString("yyyy-MM-dd HH:mm:ss")
				////
				//, "_biz._cf.nvcParams", TJson.JsonConvert.SerializeObject(_biz._cf.nvcParams)
				//, "_biz.RQ_Mst_Brand", TJson.JsonConvert.SerializeObject(objRQ_Mst_Brand)
				});
            #endregion

            try
            {
                #region // CheckGatewayAuthentication:
                TUtils.CConnectionManager.CheckGatewayAuthentication(
                    _biz._cf.nvcParams // nvcParams
                    , ref alParamsCoupleError // alParamsCoupleError
                    , objRQ_Mst_Brand.GwUserCode // strGwUserCode
                    , objRQ_Mst_Brand.GwPassword // strGwPassword
                    );
                #endregion

                #region // Process:
                //
                mdsReturn = _biz.WAS_Mst_Brand_Delete_Sync(
                    ref alParamsCoupleError // alParamsCoupleError
                    , objRQ_Mst_Brand // objRQ_Mst_Brand
                                     // //
                    , out objRT_Mst_Brand // RT_Mst_Brand
                    );

                if (CmUtils.CMyDataSet.HasError(mdsReturn))
                {
                    throw CmUtils.CMyException.Raise(
                        (string)CmUtils.CMyDataSet.GetErrorCode(mdsReturn)
                        , null
                        , null
                        );
                }
                #endregion

                // Return Good:
                objRT_Mst_Brand.c_K_DT_Sys = TUtils.CMyDataList.CProcessMyDS(mdsReturn);
                return Success<RT_Mst_Brand>(objRT_Mst_Brand);
            }
            catch (Exception ex)
            {
                #region // Catch of try:
                ////
                TUtils.CProcessExc.Process(
                    ref mdsReturn // mdsFinal
                    , ex // exc
                    , strErrorCodeDefault // strErrorCode
                    , alParamsCoupleError.ToArray() // arrobjErrorParams
                    );

                // Return Bad:
                if (objRT_Mst_Brand == null) objRT_Mst_Brand = new RT_Mst_Brand();
                objRT_Mst_Brand.c_K_DT_Sys = TUtils.CMyDataList.CProcessMyDS(mdsReturn);
                return Error<RT_Mst_Brand>(ex, objRT_Mst_Brand);
                #endregion
            }
        }

        [AcceptVerbs("POST")]
        public ServiceResult<RT_QueSync_Mst_Brand> WA_QueSync_Mst_Brand_Get(RQ_QueSync_Mst_Brand objRQ_QueSync_Mst_Brand)
        {
            #region // Temp:
            DataSet mdsReturn = CmUtils.CMyDataSet.NewMyDataSet(objRQ_QueSync_Mst_Brand.Tid);
            RT_QueSync_Mst_Brand objRT_QueSync_Mst_Brand = null;
            DateTime dtimeSys = DateTime.Now;
            string strControllerName = "WA_QueSync_Mst_Brand_Get";
            string strErrorCodeDefault = "WA_QueSync_Mst_Brand_Get";

            ArrayList alParamsCoupleError = new ArrayList();
            alParamsCoupleError.AddRange(new object[] {
                "strControllerName", strControllerName
                , "dtimeSys", dtimeSys.ToString("yyyy-MM-dd HH:mm:ss")
				////
				});
            #endregion

            try
            {
                #region // CheckGatewayAuthentication:
                TUtils.CConnectionManager.CheckGatewayAuthentication(
                    _biz._cf.nvcParams // nvcParams
                    , ref alParamsCoupleError // alParamsCoupleError
                    , objRQ_QueSync_Mst_Brand.GwUserCode // strGwUserCode
                    , objRQ_QueSync_Mst_Brand.GwPassword // strGwPassword
                    );
                #endregion

                #region // Process:
                //
                mdsReturn = _biz.WAS_QueSync_Mst_Brand_Get(
                    ref alParamsCoupleError // alParamsCoupleError
                    , objRQ_QueSync_Mst_Brand // objRQ_QueSync_Mst_Brand
                                              ////
                    , out objRT_QueSync_Mst_Brand // RT_QueSync_Mst_Brand
                    );

                if (CmUtils.CMyDataSet.HasError(mdsReturn))
                {
                    throw CmUtils.CMyException.Raise(
                        (string)CmUtils.CMyDataSet.GetErrorCode(mdsReturn)
                        , null
                        , null
                        );
                }
                #endregion

                // Return Good:
                objRT_QueSync_Mst_Brand.c_K_DT_Sys = TUtils.CMyDataList.CProcessMyDS(mdsReturn);
                return Success<RT_QueSync_Mst_Brand>(objRT_QueSync_Mst_Brand);
            }
            catch (Exception ex)
            {
                #region // Catch of try:
                ////
                TUtils.CProcessExc.Process(
                    ref mdsReturn // mdsFinal
                    , ex // exc
                    , strErrorCodeDefault // strErrorCode
                    , alParamsCoupleError.ToArray() // arrobjErrorParams
                    );

                // Return Bad:
                if (objRT_QueSync_Mst_Brand == null) objRT_QueSync_Mst_Brand = new RT_QueSync_Mst_Brand();
                objRT_QueSync_Mst_Brand.c_K_DT_Sys = TUtils.CMyDataList.CProcessMyDS(mdsReturn);
                return Error<RT_QueSync_Mst_Brand>(ex, objRT_QueSync_Mst_Brand);
                #endregion
            }
        }
    }
}
