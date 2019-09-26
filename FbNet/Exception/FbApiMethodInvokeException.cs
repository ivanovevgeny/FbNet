namespace FbNet.Exception
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Базовый класс, для всех исключений, которые могут произойти при вызове методов API Одноклассники.
    /// </summary>
    [Serializable]
    public class FbApiMethodInvokeException : FbApiException
    {
        /// <summary>
        /// Код ошибки, полученный от сервера Одноклассники.
        /// </summary>
        public int ErrorCode { get; private set; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="FbApiMethodInvokeException"/>.
        /// </summary>
        public FbApiMethodInvokeException()
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="FbApiMethodInvokeException"/> с указанным описанием.
        /// </summary>
        /// <param name="message">Описание исключения.</param>
        public FbApiMethodInvokeException(string message) : base(message)
        {
        }
        
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="FbApiMethodInvokeException"/> с указанным описанием и внутренним исключением.
        /// </summary>
        /// <param name="message">Описание исключения.</param>
        /// <param name="innerException">Внутреннее исключение.</param>
        public FbApiMethodInvokeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="FbApiMethodInvokeException"/> с указанным описанием и кодом ошибки.
        /// </summary>
        /// <param name="message">Описание исключения.</param>
        /// <param name="code">Код ошибки, полученный от сервера Одноклассники.</param>
        public FbApiMethodInvokeException(string message, int code) : base(message)
        {
            ErrorCode = code;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="FbApiMethodInvokeException"/> с указанным описанием, кодом ошибки и внутренним исключением.
        /// </summary>
        /// <param name="message">Описание исключения.</param>
        /// <param name="code">Код ошибки, полученный от сервера Одноклассники.</param>
        /// <param name="innerException">Внутреннее исключение.</param>
        public FbApiMethodInvokeException(string message, int code, Exception innerException) : base(message, innerException)
        {
            ErrorCode = code;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="FbApiMethodInvokeException"/> на основе ранее сериализованных данных.
        /// </summary>
        /// <param name="info">Содержит все данные, необходимые для десериализации.</param>
        /// <param name="context">Описывает источник и назначение данного сериализованного потока и предоставляет дополнительный, 
        /// определяемый вызывающим, контекст.</param>
        protected FbApiMethodInvokeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}