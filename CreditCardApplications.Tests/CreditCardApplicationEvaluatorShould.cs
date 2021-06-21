using System;
using Xunit;
using Moq;

namespace CreditCardApplications.Tests
{
    public class CreditCardApplicationEvaluatorShould
    {
        // ***************** Configuring Mocked Methods - Start **********************
        [Fact]
        public void AcceptHighIncomeApplications()
        {
            Mock<IFrequentFlyerNumberValidator> mockValidator =
                new Mock<IFrequentFlyerNumberValidator>();

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

            var application = new CreditCardApplication { GrossAnnualIncome = 100_000 };

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.AutoAccepted, decision);
        }

        [Fact]
        public void ReferYoungApplications()
        {
            var mockValidator =
                    new Mock<IFrequentFlyerNumberValidator>();
            // setting up default should be done carefully, as it can hide issues
            // setting this up will allow default values instead of null reference of an object
            mockValidator.DefaultValue = DefaultValue.Mock;

            mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);
            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

            var application = new CreditCardApplication { Age = 19 };

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        [Fact]
        public void DeclineLowIncomeApplications()
        {
            Mock<IFrequentFlyerNumberValidator> mockValidator =
                    new Mock<IFrequentFlyerNumberValidator>();
            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");
            // mockValidator.Setup(x => x.IsValid("x")).Returns(true); // - for specific argument matching

            // mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true); // generic argument matching

            // specific argument matching
            //mockValidator.Setup(
            //    x => x.IsValid(It.Is<string>(number => number.StartsWith("y"))))
            //    .Returns(true);

            // specific argument in a range
            //mockValidator.Setup(
            //    x => x.IsValid(It.IsInRange<string>("a", "z", Moq.Range.Inclusive)))
            //    .Returns(true);

            // inference type in a range
            //mockValidator.Setup(x => x.IsValid(It.IsInRange("a", "z", Moq.Range.Inclusive)))
            //    .Returns(true);

            // argument falls in a set
            //mockValidator.Setup(
            //    x => x.IsValid(It.IsIn("x", "y", "z")))
            //    .Returns(true);

            //regex matching
            mockValidator.Setup(
                x => x.IsValid(It.IsRegex("[a-z]{1}")))
                .Returns(true);



            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

            var application = new CreditCardApplication 
            { 
                Age = 42,
                GrossAnnualIncome = 19_999,
                FrequentFlyerNumber = "y"
            };

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.AutoDeclined, decision);
        }

        [Fact]
        public void ReferInvalidFrequentFlyerApplications()
        {
            //var mockValidator =
            //        new Mock<IFrequentFlyerNumberValidator>(MockBehavior.Strict);

            var mockValidator =
                new Mock<IFrequentFlyerNumberValidator>(MockBehavior.Default); // default is loose, or just leave empty
            mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(false);

            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

            var application = new CreditCardApplication();

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        //[Fact]
        //public void DeclineLowIncomeApplicationsOutDemo()
        //{
        //    Mock<IFrequentFlyerNumberValidator> mockValidator =
        //            new Mock<IFrequentFlyerNumberValidator>();

        //    bool isValid = true;
        //    mockValidator.Setup(x => x.IsValid(It.IsAny<string>(), out isValid));

        //    var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

        //    var application = new CreditCardApplication
        //    {
        //        Age = 42,
        //        GrossAnnualIncome = 19_999
        //    };

        //    CreditCardApplicationDecision decision = sut.Evaluate(application);

        //    Assert.Equal(CreditCardApplicationDecision.AutoDeclined, decision);
        //}

        // just an example of how to use ref in a function called Execute(), not really part of this test project
        //public void HowToMockRef()
        //{
        //    var person1 = new Person();
        //    var person2 = new Person();

        //    var mockgateway = new Mock<IGateway>();
        //    mockgateway.Setup(x => x.Execute(ref It.Ref<Person>.IsAny))
        //        .Returns(-1);

        //    var sut = new Processor(mockGateway.Object);
        //    sut.Process(person1); - returns -1
        //    sut.Process(person2); - returns -1
        //}

        // ***************** Configuring Mocked Methods - End **********************

        // ***************** Configuring Mocked Properties - Start **********************
        [Fact]
        public void ReferWhenLicenseKeyExpired()
        {
            //Mock<IFrequentFlyerNumberValidator> mockValidator =
            //        new Mock<IFrequentFlyerNumberValidator>();

            //mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);
            //mockValidator.Setup(x => x.LicenseKey).Returns(GetLicenseKeyExpiryString);
            // this property GetLicenseKeyExpiryString will be accessed at runtime when code calls it or uses it in the flow
            // and not during this setup function

            // ************* Mocking hierarchy of properties
            //var mockLicenseData = new Mock<ILicenseData>();
            //mockLicenseData.Setup(x => x.LicenseKey).Returns("EXPIRED");

            //var mockServiceInfo = new Mock<IServiceInformation>();
            //mockServiceInfo.Setup(x => x.License).Returns(mockLicenseData.Object);

            //var mockValidator = new Mock<IFrequentFlyerNumberValidator>();
            //mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);
            //mockValidator.Setup(x => x.ServiceInformation).Returns(mockServiceInfo.Object);

            // ************** Auto-Mocking properties hierarchy
            var mockValidator = new Mock<IFrequentFlyerNumberValidator>();
            mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);
            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("EXPIRED");

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

            var application = new CreditCardApplication
            {
                Age = 42
            };

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        // ***************** Configuring Mocked Properties - End **********************

        string GetLicenseKeyExpiryString()
        {
            // E.g. read from vendor supplied constants file
            return "Expired";
        }

        // ***************** Track changes to Mock Property Values - Start **********************
        [Fact]
        public void UseDetailedLookUpForOlderApplications()
        {
            var mockValidator = new Mock<IFrequentFlyerNumberValidator>();

            // By default mocked property doesn't remember changes being done to it
            // Using setuppropert will allow it to track changes to the property
            //mockValidator.SetupProperty(x => x.ValidationMode);

            // If mocked Object has a number of properties that you want to track changes for, use the following instead of individually
            mockValidator.SetupAllProperties(); // this needs to be done before any setup functions are called for mocked object

            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);
            var application = new CreditCardApplication { Age = 30 };

            sut.Evaluate(application);
            Assert.Equal(ValidationMode.Detailed, mockValidator.Object.ValidationMode);
        }

        // ***************** Track changes to Mock Property Values - End **********************



        // ***************** Configuring Mock Behavior Testing - Start **********************

        [Fact]
        public void ValidateFrequentFlyerNumberForLowIncomeApplications_VerifyMethodCalledWithArgumentNull()
        {
            var mockValidator = new Mock<IFrequentFlyerNumberValidator>();

            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);
            var application = new CreditCardApplication();

            sut.Evaluate(application);

            // verify if the mocked object method IsValid(null) for SUT is called
            mockValidator.Verify(x => x.IsValid(null));
        }

        [Fact]
        public void ValidateFrequentFlyerNumberForLowIncomeApplications_VerifyMethodCalled()
        {
            var mockValidator = new Mock<IFrequentFlyerNumberValidator>();

            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);
            var application = new CreditCardApplication
            {
                FrequentFlyerNumber = "q"
            };

            sut.Evaluate(application);

            // verify if the mocked object method IsValid(null) for SUT is called
            //mockValidator.Verify(x => x.IsValid("q"));
            //mockValidator.Verify(x => x.IsValid(It.IsAny<string>()));

            // adding a custom error message 
            mockValidator.Verify(x => x.IsValid(It.IsAny<string>()), "Frequent flyer number should be validated");
        }

        [Fact]
        public void NotValidateFrequentFlyerNumberForHighIncomeApplications_VerifyMethodNotCalled()
        {
            var mockValidator = new Mock<IFrequentFlyerNumberValidator>();

            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);
            var application = new CreditCardApplication
            {
                GrossAnnualIncome = 100_000
            };

            sut.Evaluate(application);

            // verify the method was not called
            mockValidator.Verify(x => x.IsValid(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void ValidateFrequentFlyerNumberForLowIncomeApplications_VerifyMethodCalledSpecificNoTimes()
        {
            var mockValidator = new Mock<IFrequentFlyerNumberValidator>();

            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);
            var application = new CreditCardApplication
            {
                FrequentFlyerNumber = "q"
            };

            sut.Evaluate(application);

            // verify the method was not called
            //mockValidator.Verify(x => x.IsValid(It.IsAny<string>()), Times.Exactly(2));
            mockValidator.Verify(x => x.IsValid(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void CheckLicenseKeyForLowIncomeApplications_VerifyGetProperty()
        {
            var mockValidator = new Mock<IFrequentFlyerNumberValidator>();

            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);
            var application = new CreditCardApplication
            {
                GrossAnnualIncome = 99_000
            };

            sut.Evaluate(application);

            // verify the LicenseKey property was accessed/get
            // mockValidator.VerifyGet(x => x.ServiceInformation.License.LicenseKey, Times.Never);
            mockValidator.VerifyGet(x => x.ServiceInformation.License.LicenseKey);
        }

        [Fact]
        public void SetDetailedLookupForOldApllications_VerifySetProperty()
        {
            var mockValidator = new Mock<IFrequentFlyerNumberValidator>();

            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);
            var application = new CreditCardApplication
            {
                Age = 30
            };

            sut.Evaluate(application);

            // verify the LicenseKey property was accessed/get
            // mockValidator.VerifyGet(x => x.ServiceInformation.License.LicenseKey, Times.Never);
            // mockValidator.VerifySet(x => x.ValidationMode = ValidationMode.Detailed);
            mockValidator.VerifySet(x => x.ValidationMode = It.IsAny<ValidationMode>(), Times.Once); // if set to any validationmode value

            // verify no other calls on mocked object
            //mockValidator.Verify(x => x.IsValid(null), Times.Once);
            /*mockValidator.VerifyNoOtherCalls();*/ // only true if you have verified all the other calls in the code flow
        }

        // ***************** Configuring Mock Behavior Testing - Start **********************
    }
}
