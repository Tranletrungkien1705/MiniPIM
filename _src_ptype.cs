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
    public class MstProductTypeController : ApiControllerBase
    {
        [AcceptVerbs("POST")]
        public ServiceResult<RT_Mst_ProductType> WA_Mst_ProductType_Get(RQ_Mst_ProductType objRQ_Mst_ProductType)
        {
            #region // Temp:
            DataSet mdsReturn = CmUtils.CMyDataSet.NewMyDataSet(objRQ_Mst_ProductType.Tid);
            RT_Mst_ProductType objRT_Mst_ProductType = null;
            DateTime dtimeSys = DateTime.Now;
            string strControllerName = "WA_Mst_ProductType_Get";
            string strErrorCodeDefault = "WA_Mst_ProductType_Get";

            ArrayList alParamsCoupleError = new ArrayList();
            alParamsCoupleError.AddRange(new object[] {
                "strControllerName", strControllerName
                , "dtimeSys", dtimeSys.ToString("yyyy-MM-dd HH:mm:ss")
				//, "_biz._cf.nvcParams", TJson.JsonConvert.SerializeObject(_biz._cf.nvcParams)
				//, "_biz.RQ_Mst_ProductType", TJson.JsonConvert.SerializeObject(objRQ_Mst_ProductType)
				////
				});
            #endregion

            try
            {
                #region // CheckGatewayAuthentication:
                TUtils.CConnectionManager.CheckGatewayAuthentication(
                    _biz._cf.nvcParams // nvcParams
                    , ref alParamsCoupleError // alParamsCoupleError
                    , objRQ_Mst_ProductType.GwUserCode // strGwUserCode
                    , objRQ_Mst_ProductType.GwPassword // strGwPassword
                    );
                #endregion

                #region // Process:
                //
                mdsReturn = _biz.WAS_Mst_ProductType_Get(
                    ref alParamsCoupleError // alParamsCoupleError
                    , objRQ_Mst_ProductType // objRQ_Mst_ProductType
                                     // //
                    , out objRT_Mst_ProductType // RT_Mst_ProductType
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
                objRT_Mst_ProductType.c_K_DT_Sys = TUtils.CMyDataList.CProcessMyDS(mdsReturn);
                return Success<RT_Mst_ProductType>(objRT_Mst_ProductType);
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
                if (objRT_Mst_ProductType == null) objRT_Mst_ProductType = new RT_Mst_ProductType();
                objRT_Mst_ProductType.c_K_DT_Sys = TUtils.CMyDataList.CProcessMyDS(mdsReturn);
                return Error<RT_Mst_ProductType>(ex, objRT_Mst_ProductType);
                #endregion
            }
        }

       
    }
}
