namespace Sample.Components.CourierActivities
{
    using System;
    using System.Threading.Tasks;
    using MassTransit.Courier;


    public class PaymentActivity :
        IActivity<PaymentArguments, PaymentLog>
    {
        public async Task<ExecutionResult> Execute(ExecuteContext<PaymentArguments> context)
        {
            string cardNumber = context.Arguments.CardNumber;
            if (string.IsNullOrEmpty(cardNumber))
                throw new ArgumentNullException(nameof(cardNumber));

            await Task.Delay(5000);

            if (cardNumber.StartsWith("5999"))
            {
                throw new InvalidOperationException("The card number was invalid");
            }

            await Task.Delay(300);

            return context.Completed(new {AuthorizationCode = "77777"});
        }

        public async Task<CompensationResult> Compensate(CompensateContext<PaymentLog> context)
        {
            await Task.Delay(100);

            return context.Compensated();
        }
    }
}