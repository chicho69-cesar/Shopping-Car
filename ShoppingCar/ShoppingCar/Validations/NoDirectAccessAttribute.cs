using Microsoft.AspNetCore.Mvc.Filters;

namespace ShoppingCar.Validations {
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class NoDirectAccessAttribute : ActionFilterAttribute {
        public override void OnActionExecuting(ActionExecutingContext filterContext) {
            if (filterContext.HttpContext.Request.GetTypedHeaders().Referer == null ||
                filterContext.HttpContext.Request.GetTypedHeaders().Host.Host.ToString() != filterContext.HttpContext.Request.GetTypedHeaders().Referer.Host.ToString()) {
                filterContext.HttpContext.Response.Redirect("/");
            }
        }
    }
}