using System;

namespace Penguin.WinForms.Editors.Dependencies
{
    public class RenderExceptionWrapper
    {
        private readonly Exception source;
        public string Message => this.source?.Message;
        public string StackTrace => this.source?.StackTrace;
        public RenderExceptionWrapper(Exception renderException) => this.source = renderException;
    }
}
