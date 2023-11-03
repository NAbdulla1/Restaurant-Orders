using RestaurantOrder.Core.Validations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantOrder.All.Tests
{
    [TestFixture]
    public class RequiredGuidAttributeShould
    {
        private RequiredGuidAttribute _sut;

        [SetUp]
        public void Setup()
        {
            _sut = new RequiredGuidAttribute();
        }

        [Test]
        public void SuccessfullyValidateNonEmptyGuid()
        {
            var guid = Guid.NewGuid();

            var validationResult = _sut.IsValid(guid);

            Assert.That(validationResult, Is.True);
        }

        [TestCase(null)]
        [TestCase("00000000-0000-0000-0000-000000000000")]
        public void FailValidationForEmptyGuid(Guid? guid)
        {
            var validationResult = _sut.IsValid(guid);

            Assert.That(validationResult, Is.False);
        }

        [TestCase("asdf-asdf-asdf-asdf")]
        public void FailValidationForArbitraryStringValue(string invalidGuid)
        {
            var validationResult = _sut.IsValid(invalidGuid);

            Assert.That(validationResult, Is.False);
        }
    }
}
