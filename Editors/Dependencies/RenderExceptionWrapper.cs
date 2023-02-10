using System;

namespace Penguin.WinForms.Editors.Dependencies
{
    public class RenderExceptionWrapper
    {
        private readonly Exception source;

        public string Message => source?.Message;

        public string StackTrace => source?.StackTrace;

        public RenderExceptionWrapper(Exception renderException)
        {
            source = renderException;
        }
    }
}