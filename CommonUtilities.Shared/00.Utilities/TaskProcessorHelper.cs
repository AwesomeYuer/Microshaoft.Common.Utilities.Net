#if NETFRAMEWORK4_X
namespace Test2
{
    using System;
    using System.Windows.Forms;
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
namespace Test2
{
    using System;
    using System.Threading;
    using System.Windows.Forms;
    using Microshaoft;
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
#region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(119, 74);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(282, 253);
            this.Controls.Add(this.button1);
            this.Name = "MainForm";
            this.Text = "MainForm";
            this.ResumeLayout(false);
        }
#endregion
        private System.Windows.Forms.Button button1;
    }
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            var result = false;
            var r = TaskProcessorHelper
                            .ProcessWaitingShowDialog
                                    (
                                        this
                                        , new ProcessWaitingCancelableDialog()
                                        , () =>
                                        {

                                            Thread.Sleep(5 * 1000);
                                            //throw new Exception();
                                            result = true;
                                        }

                                        , (x) =>
                                        {
                                            Console.WriteLine("Caught Exception: {0}", x);
                                        }
                                    );

            //if (r == DialogResult.Cancel)
            {
                Console.WriteLine(r);
                Console.WriteLine(result);


            }

            Console.WriteLine(r);
        }
    }
}
#endif
namespace Microshaoft
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
#if NETFRAMEWORK4_X
    using System.Windows.Forms;
#endif
    public static class TaskProcessorHelper
    {
#if NETFRAMEWORK4_X

        public static int ProcessWaitingShowDialog
                    (
                        IWin32Window ownerWindow
                        , Func<Form> onWaitingDialogFactoryFunc
                        , Action onProcessAction = null
                        , Action onProcessedAction = null
                        , Action<Exception> onCaughtExceptionProcessAction = null
                    )
        {
            var dialogForm = onWaitingDialogFactoryFunc();
            return
                ProcessWaitingShowDialog
                            (
                                ownerWindow
                                , dialogForm
                                , onProcessAction = null
                                , onProcessedAction = null
                                , onCaughtExceptionProcessAction = null
                            );
        }
        public static int ProcessWaitingShowDialog40
                            (
                                IWin32Window ownerWindow
                                , Form dialogForm
                                , Action onProcessAction = null
                                , Action onProcessedAction = null
                                , Action<Exception> onCaughtExceptionProcessAction = null
                            )
        {
            var r = 1;
            Task<DialogResult> task1 = Task.Factory.StartNew<DialogResult>
                            (
                                () =>
                                {
                                    return dialogForm.ShowDialog();
                                }
                            );
            Task task2 = Task.Factory.StartNew
                                    (
                                        () =>
                                        {
                                            try
                                            {
                                                //
                                                onProcessAction();
                                                r = 0;
                                            }
                                            catch (Exception e)
                                            {
                                                r = -1;
                                                if (onCaughtExceptionProcessAction != null)
                                                {
                                                    onCaughtExceptionProcessAction(e);
                                                }
                                            }
                                            finally
                                            {
                                                TrySafeInvokeFormClose
                                                    (
                                                        dialogForm
                                                        , onCaughtExceptionProcessAction
                                                    );
                                            }
                                            try
                                            {
                                                onProcessedAction();
                                            }
                                            catch (Exception e)
                                            {
                                                //r = -1;
                                                onCaughtExceptionProcessAction(e);
                                            }
                                            finally
                                            {
                                                TrySafeInvokeFormClose(dialogForm, onCaughtExceptionProcessAction);
                                            }
                                        }

                                    );
            Task.WaitAny(task1, task2);
            //DialogResult dialogResult = await task;
            return r;
        }

        public static T ProcessWaitingShowDialog<T>
                    (
                        IWin32Window ownerWindow
                        , Form dialogForm
                        , Func<T> onProcessFunc
                        , out DialogResult dialogResult
                        , Func<Exception, Exception, string, bool> onCaughtExceptionProcessFunc = null
                        , Action<bool, Exception, Exception, string> onFinallyProcessAction = null
                    )
        {
            T r = default(T);
            dialogResult = default(DialogResult);
            var IsCompleted = false;
            new Thread
                (
                    new ThreadStart
                        (
                            () =>
                            {
                                //wait.WaitOne();
                                Thread.Sleep(10);
                                TryCatchFinallyProcessHelper
                                    .TryProcessCatchFinally
                                        (
                                            true
                                            , () =>
                                            {
                                                r = onProcessFunc();
                                                IsCompleted = true;
                                            }
                                            , false
                                            , onCaughtExceptionProcessFunc
                                            , onFinallyProcessAction
                                        );
                            }
                        )
                ).Start();

            if (!IsCompleted)
            {
                dialogResult = dialogForm.ShowDialog(ownerWindow);
            }
            return r;
        }


        public static DialogResult ProcessWaitingShowDialog
                    (
                        IWin32Window ownerWindow
                        , Form dialogForm
                        , Action onProcessAction = null
                        , Action<Exception> onCaughtExceptionProcessAction = null
                    )
        {
            //var wait = new AutoResetEvent(false);
            DialogResult r = default(DialogResult);
            var IsCompleted = false;
            if (onProcessAction != null)
            {
                new Thread
                        (
                            new ThreadStart
                                (
                                    () =>
                                    {
                                        //wait.WaitOne();
                                        Thread.Sleep(10);
                                        try
                                        {
                                            //
                                            onProcessAction();
                                            IsCompleted = true;
                                        }
                                        catch (Exception e)
                                        {
                                            //r = -1;
                                            if (onCaughtExceptionProcessAction != null)
                                            {
                                                onCaughtExceptionProcessAction(e);
                                            }
                                        }
                                        finally
                                        {
                                            TrySafeInvokeFormClose
                                                (
                                                    dialogForm
                                                    , onCaughtExceptionProcessAction
                                                );
                                        }
                                    }
                                )
                        ).Start();
                //wait.Set();
                if (!IsCompleted)
                {
                    r = dialogForm.ShowDialog(ownerWindow);
                }
            }
            return r;
        }

        public static int ProcessWaitingShowDialog
                            (
                                IWin32Window ownerWindow
                                , Form dialogForm
                                , Action onProcessAction = null
                                , Action onProcessedAction = null
                                , Action<Exception> onCaughtExceptionProcessAction = null
                            )
        {
            //var wait = new AutoResetEvent(false);
            int r = 1;
            if (onProcessAction != null)
            {
                new Thread
                        (
                            new ThreadStart
                                (
                                    () =>
                                    {
                                        //wait.WaitOne();
                                        Thread.Sleep(10);
                                        try
                                        {
                                            //
                                            onProcessAction();
                                            r = 0;
                                        }
                                        catch (Exception e)
                                        {
                                            r = -1;
                                            if (onCaughtExceptionProcessAction != null)
                                            {
                                                onCaughtExceptionProcessAction(e);
                                            }
                                        }
                                        finally
                                        {
                                            TrySafeInvokeFormClose
                                                (
                                                    dialogForm
                                                    , onCaughtExceptionProcessAction
                                                );
                                        }
                                        try
                                        {
                                            onProcessedAction();
                                        }
                                        catch (Exception e)
                                        {
                                            //r = -1;
                                            onCaughtExceptionProcessAction(e);
                                        }
                                        finally
                                        {
                                            TrySafeInvokeFormClose(dialogForm, onCaughtExceptionProcessAction);
                                        }
                                    }
                                )
                        ).Start();
                //wait.Set();
                if (r != 0)
                {
                    dialogForm.ShowDialog(ownerWindow);
                }
            }
            return r;
        }

        private static bool TrySafeInvokeFormClose
                                (
                                    Form dialogForm
                                    , Action<Exception> onCaughtExceptionProcessAction
                                )
        {
            bool r = false;
            try
            {
                if
                (
                    dialogForm.IsHandleCreated
                    && !dialogForm.IsDisposed
                )
                {
                    dialogForm.Invoke
                            (
                                new Action
                                    (
                                        () =>
                                        {
                                            //try
                                            {
                                                if
                                                (
                                                    dialogForm.IsHandleCreated
                                                    && !dialogForm.IsDisposed
                                                )
                                                {
                                                    dialogForm.Close();
                                                }
                                                //throw new Exception("理论上不应该被外侧 try catch 捕获?!?!?!?!?!");
                                            }
                                            ///											catch (Exception e)
                                            ///											{
                                            ///												r = false;
                                            ///												if (onCaughtExceptionProcessAction != null)
                                            ///												{
                                            ///													onCaughtExceptionProcessAction(e);
                                            ///												}
                                            ///											}
                                        }
                                    )
                            );
                    Thread.Sleep(10);
                }
                r = true;
            }
            catch (Exception e)
            {
                r = false;
                if (onCaughtExceptionProcessAction != null)
                {
                    onCaughtExceptionProcessAction(e);
                }
            }
            return r;
        }
#endif
        public static int ProcessWaitingCancelable
                            (
                                Func<AutoResetEvent> onWaitFactoryFunc
                                , Action onProcessAction
                                , Action onProcessedAction
                                , Action<Exception> onCaughtExceptionProcessAction
                            )
        {
            var wait = onWaitFactoryFunc();
            return
                ProcessWaitingCancelable
                            (
                                wait
                                , onProcessAction
                                , onProcessedAction
                                , onCaughtExceptionProcessAction
                            );
        }
        public static int ProcessWaitingCancelable
                            (
                                AutoResetEvent wait
                                , Action onProcessAction
                                , Action onProcessedAction
                                , Action<Exception> onCaughtExceptionProcessAction
                            )
        {
            int r = 1; //Cancel
            new Thread
                    (
                        new ThreadStart
                                (
                                    () =>
                                    {
                                        try
                                        {
                                            onProcessAction();
                                            r = 0;
                                            onProcessedAction();
                                        }
                                        catch (Exception e)
                                        {
                                            r = -1;
                                            onCaughtExceptionProcessAction(e);
                                        }
                                        finally
                                        {
                                            wait.Set();
                                        }
                                    }
                                )
                    ).Start();
            wait.WaitOne();
            return r;
        }
    }
}
#if NETFRAMEWORK4_X

namespace Microshaoft
{
    using System;
    using System.Drawing;
    using System.ComponentModel;
    using System.Threading;
    using System.Windows.Forms;
    public class ProcessWaitingCancelableDialog : Form
    {
        private IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        private void InitializeComponent()
        {
            button1 = new Button();
            SuspendLayout();
            //
            // button1
            //
            button1.DialogResult = DialogResult.Cancel;
            button1.Location = new Point(98, 158);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 0;
            button1.Text = "取消(&C)";
            button1.UseVisualStyleBackColor = true;
            //
            // MainForm
            //
            AutoScaleDimensions = new SizeF(8F, 16F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = button1;
            ClientSize = new Size(282, 253);
            ControlBox = false;
            Controls.Add(button1);
            ///Name = "MainForm";
            ///Text = "MainForm";
            ResumeLayout(false);
        }
        private Button button1;
        public Button CancelWaitButton
        {
            get
            {
                return button1;
            }
        }
        public ProcessWaitingCancelableDialog()
        {
            InitializeComponent();
            button1.Click += button1_Click;
        }
        void button1_Click(object sender, EventArgs e)
        {
            button1.Click -= button1_Click;
            Close();
        }
    }
}
#endif