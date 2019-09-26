namespace FbNet.Exception
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Базовый класс для всех исключений, выбрасываемых библиотекой.
    /// </summary>
    [Serializable]
    public class FbApiException : Exception
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="FbApiException"/>.
        /// </summary>
        public FbApiException()
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="FbApiException"/> с указанным описанием.
        /// </summary>
        /// <param name="message">Описание исключения.</param>
        public FbApiException(string message) : base(message)
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="InvalidParameterException"/> с указанным описанием и внутренним исключением.
        /// </summary>
        /// <param name="message">Описание исключения.</param>
        /// <param name="innerException">Внутреннее исключение.</param>
        public FbApiException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="FbApiException"/> на основе ранее сериализованных данных.
        /// </summary>
        /// <param name="info">Содержит все данные, необходимые для десериализации.</param>
        /// <param name="context">Описывает источник и назначение данного сериализованного потока и предоставляет дополнительный, 
        /// определяемый вызывающим, контекст.</param>
        protected FbApiException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}