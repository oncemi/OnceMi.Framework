using Castle.DynamicProxy;
using OnceMi.Framework.IRepository;
using OnceMi.Framework.Model.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Extension.Aop
{
    public class TransactionAsyncInterceptor : ITransactionAsyncInterceptor
    {
        private readonly BaseUnitOfWorkManager _uowManager;

        public TransactionAsyncInterceptor(BaseUnitOfWorkManager uowManager)
        {
            _uowManager = uowManager ?? throw new ArgumentNullException(nameof(uowManager));
        }

        public (bool result, List<IAopAttribute> attrs) CanIntercept(IInvocation invocation)
        {
            List<IAopAttribute> attrs = invocation
                .MethodInvocationTarget
                .GetCustomAttributes(typeof(TransactionAttribute), false)
                ?.Select(p => p as IAopAttribute)
                ?.ToList();
            if (attrs == null || attrs.Count == 0)
            {
                return (false, null);
            }
            return (true, attrs);
        }

        public void InterceptAsynchronous(IInvocation invocation)
        {
            invocation.ReturnValue = InternalInterceptAsynchronous(invocation);
        }

        public void InterceptAsynchronous<TResult>(IInvocation invocation)
        {
            invocation.ReturnValue = InternalInterceptAsynchronous<TResult>(invocation);
        }

        public void InterceptSynchronous(IInvocation invocation)
        {
            InternalInterceptSynchronous(invocation);
        }

        #region private

        private void InternalInterceptSynchronous(IInvocation invocation)
        {
            var (result, attrs) = CanIntercept(invocation);
            if (result)
            {
                TransactionAttribute attr = attrs[0] as TransactionAttribute;
                using (var uow = _uowManager.Begin(attr.Propagation, attr.IsolationLevel))
                {
                    try
                    {
                        invocation.Proceed();
                        uow.Commit();
                    }
                    catch
                    {
                        uow.Rollback();
                        throw;
                    }
                }
            }
            else
            {
                invocation.Proceed();
            }
        }

        private async Task InternalInterceptAsynchronous(IInvocation invocation)
        {
            var (result, attrs) = CanIntercept(invocation);
            if (result)
            {
                TransactionAttribute attr = attrs[0] as TransactionAttribute;
                using (var uow = _uowManager.Begin(attr.Propagation, attr.IsolationLevel))
                {
                    try
                    {
                        invocation.Proceed();
                        //处理Task返回一个null值的情况会导致空指针
                        if (invocation.ReturnValue != null)
                        {
                            await (Task)invocation.ReturnValue;
                        }
                        uow.Commit();
                    }
                    catch
                    {
                        uow.Rollback();
                        throw;
                    }
                }
            }
            else
            {
                invocation.Proceed();
                //处理Task返回一个null值的情况会导致空指针
                if (invocation.ReturnValue != null)
                {
                    await (Task)invocation.ReturnValue;
                }
            }
        }

        private async Task<TResult> InternalInterceptAsynchronous<TResult>(IInvocation invocation)
        {
            var (result, attrs) = CanIntercept(invocation);
            if (result)
            {
                TransactionAttribute attr = attrs[0] as TransactionAttribute;
                using (var uow = _uowManager.Begin(attr.Propagation, attr.IsolationLevel))
                {
                    try
                    {
                        invocation.Proceed();
                        TResult returnValye = await (Task<TResult>)invocation.ReturnValue;

                        uow.Commit();
                        return returnValye;
                    }
                    catch
                    {
                        uow.Rollback();
                        throw;
                    }
                }
            }
            else
            {
                invocation.Proceed();
                TResult returnValye = await (Task<TResult>)invocation.ReturnValue;
                return returnValye;
            }
        }

        #endregion
    }
}
