using Castle.DynamicProxy;

namespace CastleCoreTest
{
    public class LogInterceptor : IInterceptor
    {
        private readonly ILogger<LogInterceptor> _logger;

        public LogInterceptor(
        ILogger<LogInterceptor> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 例外處理內容
        /// </summary>
        /// <param name="invocation"></param>
        public void Intercept(IInvocation invocation)
        {
            //初始化NLog
            //var logger = LogManager.GetLogger(invocation.GetType().FullName);
            try
            {
                _logger.LogInformation($"{invocation.MethodInvocationTarget.DeclaringType.FullName}.{invocation.Method.Name} Start");
                //執行方法
                invocation.Proceed();
                _logger.LogInformation("finish");
            }
            catch (Exception ex)
            {
                //若有例外發生，進行下列處理
                //若回傳值類型不為void
                if (invocation.Method.ReturnType != typeof(void))
                {
                    try
                    {
                        //嘗試傳回初始值
                        invocation.ReturnValue =
                            Activator.CreateInstance(invocation.Method.ReturnType);
                    }
                    catch
                    {
                        //失敗則傳回null
                        invocation.ReturnValue = ex;
                    }
                    finally
                    {
                        _logger.LogError(ex, $"{invocation.Method.Name} failed.");
                    }
                }
            }
        }
    }
}
