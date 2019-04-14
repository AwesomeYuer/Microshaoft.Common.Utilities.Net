namespace ActivityLibrary2
{
    using System;
    using System.Activities;
    public sealed class CodeActivity2 : CodeActivity
    {
        // Define an activity input argument of type string
        public InArgument<string> Text { get; set; }

        // If your activity returns a value, derive from CodeActivity<TResult>
        // and return the value from the Execute method.
        protected override void Execute(CodeActivityContext context)
        {
            // Obtain the runtime value of the Text input argument
            string text = context.GetValue(this.Text);
            Console.WriteLine(this.GetType().Name);
        }
    }
}
