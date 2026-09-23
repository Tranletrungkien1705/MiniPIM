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
    public class MstVATRateController : ApiControllerBase
    {
        [AcceptVerbs("POST")]
        public ServiceResult<RT_Mst_VATRate> WA_Mst_VATRate_Get(RQ_Mst_VATRate objRQ_Mst_VATRate)
        {
            #region // Temp:
            DataSet mdsReturn = CmUtils.CMyDataSet.NewMyDataSet(objRQ_Mst_VATRate.Tid);
            RT_Mst_VATRate objRT_Mst_VATRate = null;
            DateTime dtimeSys = DateTime.Now;
            string strControllerName = "WA_Mst_VATRate_Get";
            string strErrorCodeDefault = "WA_Mst_VATRate_Get";

            ArrayList alParamsCoupleError = new ArrayList();
            alParamsCoupleError.AddRange(new object[] {
                "strControllerName", strControllerName
                , "dtimeSys", dtimeSys.ToString("yyyy-MM-dd HH:mm:ss")
				//, "_biz._cf.nvcParams", TJson.JsonConvert.SerializeObject(_biz._cf.nvcParams)
				//, "_biz.RQ_Mst_VATRate", TJson.JsonConvert.SerializeObject(objRQ_Mst_VATRate)
				////
				});
            #endregion

            try
            {
                #region // CheckGatewayAuthentication:
                TUtils.CConnectionManager.CheckGatewayAuthentication(
                    _biz._cf.nvcParams // nvcParams
                    , ref alParamsCoupleError // alParamsCoupleError
                    , objRQ_Mst_VATRate.GwUserCode // strGwUserCode
                    , objRQ_Mst_VATRate.GwPassword // strGwPassword
                    );
                #endregion

                #region // Process:
                //
                mdsReturn = _biz.WAS_Mst_VATRate_Get(
                    ref alParamsCoupleError // alParamsCoupleError
                    , objRQ_Mst_VATRate // objRQ_Mst_VATRate
                                          // //
                    , out objRT_Mst_VATRate // RT_Mst_VATRate
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
                objRT_Mst_VATRate.c_K_DT_Sys = TUtils.CMyDataList.CProcessMyDS(mdsReturn);
                return Success<RT_Mst_VATRate>(objRT_Mst_VATRate);
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
                if (objRT_Mst_VATRate == null) objRT_Mst_VATRate = new RT_Mst_VATRate();
                objRT_Mst_VATRate.c_K_DT_Sys = TUtils.CMyDataList.CProcessMyDS(mdsReturn);
                return Error<RT_Mst_VATRate>(ex, objRT_Mst_VATRate);
                #endregion
            }
        }

        [AcceptVerbs("POST")]
        public ServiceResult<RT_Mst_VATRate> WA_Mst_VATRate_Create(RQ_Mst_VATRate objRQ_Mst_VATRate)
        {
            #region // Temp:
            DataSet mdsReturn = CmUtils.CMyDataSet.NewMyDataSet(objRQ_Mst_VATRate.Tid);
            RT_Mst_VATRate objRT_Mst_VATRate = null;
            DateTime dtimeSys = DateTime.Now;
            string strControllerName = "WA_Mst_VATRate_Creat";
            string strErrorCodeDefault = "WA_Mst_VATRate_Creat";

            ArrayList alParamsCoupleError = new ArrayList();
            alParamsCoupleError.AddRange(new object[] {
                "strControllerName", strControllerName
                , "dtimeSys", dtimeSys.ToString("yyyy-MM-dd HH:mm:ss")
				////
				//, "_biz._cf.nvcParams", TJson.JsonConvert.SerializeObject(_biz._cf.nvcParams)
				//, "_biz.RQ_Mst_VATRate", TJson.JsonConvert.SerializeObject(objRQ_Mst_VATRate)
				});
            #endregion

            try
            {
                #region // CheckGatewayAuthentication:
                TUtils.CConnectionManager.CheckGatewayAuthentication(
                    _biz._cf.nvcParams // nvcParams
                    , ref alParamsCoupleError // alParamsCoupleError
                    , objRQ_Mst_VATRate.GwUserCode // strGwUserCode
                    , objRQ_Mst_VATRate.GwPassword // strGwPassword
                    );
                #endregion

                #region // Process:
                //
                mdsReturn = _biz.WAS_Mst_VATRate_Create(
                    ref alParamsCoupleError // alParamsCoupleError
                    , objRQ_Mst_VATRate // objRQ_Mst_VATRate
                                          // //
                    , out objRT_Mst_VATRate // RT_Mst_VATRate
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
                objRT_Mst_VATRate.c_K_DT_Sys = TUtils.CMyDataList.CProcessMyDS(mdsReturn);
                return Success<RT_Mst_VATRate>(objRT_Mst_VATRate);
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
                if (objRT_Mst_VATRate == null) objRT_Mst_VATRate = new RT_Mst_VATRate();
                objRT_Mst_VATRate.c_K_DT_Sys = TUtils.CMyDataList.CProcessMyDS(mdsReturn);
                return Error<RT_Mst_VATRate>(ex, objRT_Mst_VATRate);
                #endregion
            }
        }

        [AcceptVerbs("POST")]
        public ServiceResult<RT_Mst_VATRate> WA_Mst_VATRate_Update(RQ_Mst_VATRate objRQ_Mst_VATRate)
        {
            #region // Temp:
            DataSet mdsReturn = CmUtils.CMyDataSet.NewMyDataSet(objRQ_Mst_VATRate.Tid);
            RT_Mst_VATRate objRT_Mst_VATRate = null;
            DateTime dtimeSys = DateTime.Now;
            string strControllerName = "WA_Mst_VATRate_Update";
            string strErrorCodeDefault = "WA_Mst_VATRate_Update";

            ArrayList alParamsCoupleError = new ArrayList();
            alParamsCoupleError.AddRange(new object[] {
                "strControllerName", strControllerName
                , "dtimeSys", dtimeSys.ToString("yyyy-MM-dd HH:mm:ss")
				//, "_biz._cf.nvcParams", TJson.JsonConvert.SerializeObject(_biz._cf.nvcParams)
				//, "_biz.RQ_Mst_VATRate", TJson.JsonConvert.SerializeObject(objRQ_Mst_VATRate)
				////
				});
            #endregion

            try
            {
                #region // CheckGatewayAuthentication:
                TUtils.CConnectionManager.CheckGatewayAuthentication(
                    _biz._cf.nvcParams // nvcParams
                    , ref alParamsCoupleError // alParamsCoupleError
                    , objRQ_Mst_VATRate.GwUserCode // strGwUserCode
                    , objRQ_Mst_VATRate.GwPassword // strGwPassword
                    );
                #endregion

                #region // Process:
                //
                mdsReturn = _biz.WAS_Mst_VATRate_Update(
                    ref alParamsCoupleError // alParamsCoupleError
                    , objRQ_Mst_VATRate // objRQ_Mst_VATRate
                                          // //
                    , out objRT_Mst_VATRate // RT_Mst_VATRate
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
                objRT_Mst_VATRate.c_K_DT_Sys = TUtils.CMyDataList.CProcessMyDS(mdsReturn);
                return Success<RT_Mst_VATRate>(objRT_Mst_VATRate);
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
                if (objRT_Mst_VATRate == null) objRT_Mst_VATRate = new RT_Mst_VATRate();
                objRT_Mst_VATRate.c_K_DT_Sys = TUtils.CMyDataList.CProcessMyDS(mdsReturn);
                return Error<RT_Mst_VATRate>(ex, objRT_Mst_VATRate);
                #endregion
            }
        }

        [AcceptVerbs("POST")]
        public ServiceResult<RT_Mst_VATRate> WA_Mst_VATRate_Delete(RQ_Mst_VATRate objRQ_Mst_VATRate)
        {
            #region // Temp:
            DataSet mdsReturn = CmUtils.CMyDataSet.NewMyDataSet(objRQ_Mst_VATRate.Tid);
            RT_Mst_VATRate objRT_Mst_VATRate = null;
            DateTime dtimeSys = DateTime.Now;
            string strControllerName = "WA_Mst_VATRate_Delete";
            string strErrorCodeDefault = "WA_Mst_VATRate_Delete";

            ArrayList alParamsCoupleError = new ArrayList();
            alParamsCoupleError.AddRange(new object[] {
                "strControllerName", strControllerName
                , "dtimeSys", dtimeSys.ToString("yyyy-MM-dd HH:mm:ss")
				////
				//, "_biz._cf.nvcParams", TJson.JsonConvert.SerializeObject(_biz._cf.nvcParams)
				//, "_biz.RQ_Mst_VATRate", TJson.JsonConvert.SerializeObject(objRQ_Mst_VATRate)
				});
            #endregion

            try
            {
                #region // CheckGatewayAuthentication:
                TUtils.CConnectionManager.CheckGatewayAuthentication(
                    _biz._cf.nvcParams // nvcParams
                    , ref alParamsCoupleError // alParamsCoupleError
                    , objRQ_Mst_VATRate.GwUserCode // strGwUserCode
                    , objRQ_Mst_VATRate.GwPassword // strGwPassword
                    );
                #endregion

                #region // Process:
                //
                mdsReturn = _biz.WAS_Mst_VATRate_Delete(
                    ref alParamsCoupleError // alParamsCoupleError
                    , objRQ_Mst_VATRate // objRQ_Mst_VATRate
                                          // //
                    , out objRT_Mst_VATRate // RT_Mst_VATRate
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
                objRT_Mst_VATRate.c_K_DT_Sys = TUtils.CMyDataList.CProcessMyDS(mdsReturn);
                return Success<RT_Mst_VATRate>(objRT_Mst_VATRate);
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
                if (objRT_Mst_VATRate == null) objRT_Mst_VATRate = new RT_Mst_VATRate();
                objRT_Mst_VATRate.c_K_DT_Sys = TUtils.CMyDataList.CProcessMyDS(mdsReturn);
                return Error<RT_Mst_VATRate>(ex, objRT_Mst_VATRate);
                #endregion
            }
        }
    }
}
