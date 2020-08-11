using System;

namespace App.Boilerplate.Models
{
    public class ErrorPage
    {
        public Exception Exception { get; }

        public ErrorPage(Exception exception = null)
        {
            Exception = exception;
        }
    }
}