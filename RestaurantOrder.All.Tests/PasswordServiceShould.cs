using Restaurant_Orders.Services;

namespace RestaurantOrder.All.Tests
{
    [TestFixture]
    public class PasswordServiceShould
    {
        private PasswordService sut;

        [SetUp]
        public void SetUp()
        {
            sut = new PasswordService();
        }

        [TestCase("**aPassWord23$$#")]
        [TestCase("1234")]
        public void SaltPasswordsToGetDifferentHashEvenIfThePasswordsAreSame(string password)
        {
            var passwordHash1 = sut.HashPassword(password);
            var passwordHash2 = sut.HashPassword(password);

            Assert.That(passwordHash2, Is.Not.EqualTo(passwordHash1));
        }

        [Test]
        public void BeAbleToHashAndVerifyPasswords()
        {
            var password = "aPassWord23$$";

            var hashedPassword = sut.HashPassword(password);
            var passwordVerified = sut.VerifyPassword(hashedPassword, password);

            Assert.That(passwordVerified, Is.True);
        }

        [TestCase("aPassWord23$$", "anotherPass")]
        [TestCase("anotherPass", "aPassWord23$$")]
        [TestCase("aPassWord23$$", "aPassword23$$")]
        [TestCase("aPassword23$$", "aPassWord23$$")]
        [TestCase("123", "1234")]
        [TestCase("4321", "1234")]
        [TestCase("1234", "4321")]
        public void FailToVerifyWrongPasswords(string password, string worongPassword)
        {
            var hashedPassword = sut.HashPassword(password);
            var passwordVerified = sut.VerifyPassword(hashedPassword, worongPassword);

            Assert.That(passwordVerified, Is.False);
        }
    }
}
