using System;
using System.Collections.Generic;
using System.Text;

namespace Penguin.WinForms.Editors.Dependencies
{
    public class RenderExceptionWrapper
    {
        private Exception source;
        public string Message => source?.Message;
        public string StackTrace => source?.StackTrace;
        public RenderExceptionWrapper(Exception renderException)
        {
            source = renderException;
        }
    }
}
