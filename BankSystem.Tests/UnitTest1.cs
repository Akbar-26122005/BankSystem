using Moq;
using Moq.Protected;
using NUnit.Framework;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BankSystem.Tests {
    [TestFixture]
    public class CurrencyConverterTests {
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private HttpClient _httpClient;
        private CurrencyConverter _currencyConverter;

        [SetUp]
        public void Setup() {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            _currencyConverter = new CurrencyConverter(_httpClient);
        }

        [TearDown]
        public void TearDown() {
            _httpClient.Dispose();
        }

        [Test]
        public async Task Convert_ValidCurrencies_ReturnsConvertedAmount() {
            string fromCurrency = "USD";
            string toCurrency = "EUR";
            decimal amount = 100m;
            string jsonResponse = "{\"rates\":{\"EUR\":0.85}}";

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse),
                });

            decimal result = await _currencyConverter.Convert(fromCurrency, toCurrency, amount);

            Assert.That(result, Is.EqualTo(85m));
        }

        [Test]
        public void Convert_NullHttpClient_ThrowsArgumentNullException() {
            Assert.Throws<ArgumentNullException>(() => new CurrencyConverter(null!));
        }
    }
}