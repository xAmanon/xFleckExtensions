using System;
using System.Collections.Generic;
using System.Text;

namespace Fleck.Extensions
{

    public abstract class ReturnMessage: Message
    {
        public string targetOp { get; set; }
        public bool isSuccess { get; set; }

        public ReturnMessage()
        {
            this.op = "op.return";
        }

        public ReturnMessage(string targetId, string targetOp) : this()
        {
            this.id = targetId;
            this.targetOp = targetOp;
            this.time = Utils.GetTimestemp();
        }
    }

    public class VoidReturnMesage: ReturnMessage
    {
        public VoidReturnMesage() : base()
        {
            this.isSuccess = true;
        }
    }

    public class DataReturnMessage : ReturnMessage
    {
        public object data { get; set; }

        public DataReturnMessage() : base()
        {
            this.isSuccess = true;
        }
    }

    public class ErrorReturnMessage : ReturnMessage
    {
        public int error { get; set; }
        public string message { get; set; }

        public ErrorReturnMessage() : base()
        {
            this.isSuccess = false;
        }
    }

}
