namespace UnifiedServiceScheduler.CustomException
{
    public class AppointmentBookingException : Exception
    {
        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; }

        public AppointmentBookingException(int errorCode,String message): base(message)
        {
            ErrorCode = errorCode;
            ErrorMessage = message;
        }
    }
}
