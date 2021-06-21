using System;
using Xunit;
using Moq;
using System.Collections.Generic;
using Moq.Protected;

namespace CreditCardApplications.Tests
{
    public class CreditCardApplicationEvaluatorShould
    {
        private Mock<IFrequentFlyerNumberValidator> mockValidator;
        private CreditCardApplicationEvaluator sut;

        public CreditCardApplicationEvaluatorShould()
        {
            mockValidator = new Mock<IFrequentFlyerNumberValidator>();
            mockValidator.SetupAllProperties();
            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");
            mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);

            sut = new CreditCardApplicationEvaluator(mockValidator.Object);
        }

        // ***************** Configuring Mocked Methods - Start **********************
        [Fact]
        public void AcceptHighIncomeApplications()
        {

            var application = new CreditCardApplication { GrossAnnualIncome = 100_000 };

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.AutoAccepted, decision);
        }

        [Fact]
        public void ReferYoungApplications()
        {

            // setting up default should be done carefully, as it can hide issues
            // setting this up will allow default values instead of null reference of an object
            mockValidator.DefaultValue = DefaultValue.Mock;

            var application = new CreditCardApplication { Age = 19 };

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        [Fact]
        public void DeclineLowIncomeApplications()
        {
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

        // ***************** Configuring Mock Behavior Testing - End **********************

        // ***************** Throwing Exceptions from Mocked Objects - Start **********************

        [Fact]
        public void ReferWhenFrequentFlyerValidationError()
        {
            var mockValidator = new Mock<IFrequentFlyerNumberValidator>();

            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");
            // mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Throws<Exception>();
            mockValidator
                .Setup(x => x.IsValid(It.IsAny<string>()))
                .Throws(new Exception("Custom Message"));  // throws exception with custom message

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);
            var application = new CreditCardApplication
            {
                Age = 42
            };

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);

        }

        // ***************** Throwing Exceptions from Mocked Objects - End ************************

        // ***************** Raising Events from Mocked Objects - Start **********************

        [Fact]
        public void IncrementLookupCount()
        {
            var mockValidator = new Mock<IFrequentFlyerNumberValidator>();

            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");
            mockValidator.Setup(x => x.IsValid(It.IsAny<string>()))
                .Returns(true)
                .Raises(x => x.ValidatorLookupPerformed += null, EventArgs.Empty);


            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);
            var application = new CreditCardApplication
            {
                FrequentFlyerNumber = "x",
                Age = 25
            };

            sut.Evaluate(application);

            //mockValidator.Raise(x => x.ValidatorLookupPerformed += null, EventArgs.Empty);
            Assert.Equal(1, sut.ValidatorLookupCount);

        }

        // ***************** Raising Events from Mocked Objects - End ***********************

        // ***************** Returning different results from sequential calls - Start **********************

        [Fact]
        public void ReferInvalidFrequentFlyerApplications_ReturnValuesSequence()
        {
            Mock<IFrequentFlyerNumberValidator> mockValidator = new Mock<IFrequentFlyerNumberValidator>();

            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");
            //mockValidator.Setup(x => x.IsValid(It.IsAny<string>()))
            //    .Returns(false);
            mockValidator.SetupSequence(x => x.IsValid(It.IsAny<string>()))
                .Returns(false)
                .Returns(true);



            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);
            var application = new CreditCardApplication
            {
                Age = 25
            };

            CreditCardApplicationDecision firstDecision = sut.Evaluate(application);
            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, firstDecision);

            CreditCardApplicationDecision secondDecision = sut.Evaluate(application);
            Assert.Equal(CreditCardApplicationDecision.AutoDeclined, secondDecision);

        }


        [Fact]
        public void ReferInvalidFrequentFlyerApplications_MultipleCallsSequence()
        {
            Mock<IFrequentFlyerNumberValidator> mockValidator = new Mock<IFrequentFlyerNumberValidator>();
            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");

            var frequentFlyerNumbersPassed = new List<string>();
            // capture parameters passed in InvValid() function arguments in the string list
            mockValidator.Setup(x => x.IsValid(Capture.In(frequentFlyerNumbersPassed)));

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

            var application1 = new CreditCardApplication { Age = 25, FrequentFlyerNumber = "aa" };
            var application2 = new CreditCardApplication { Age = 25, FrequentFlyerNumber = "bb" };
            var application3 = new CreditCardApplication { Age = 25, FrequentFlyerNumber = "cc" };

            sut.Evaluate(application1);
            sut.Evaluate(application2);
            sut.Evaluate(application3);

            // Assert that IsValid method was called three times "aa", "bb", and "cc"
            Assert.Equal(new List<string> { "aa", "bb", "cc" }, frequentFlyerNumbersPassed);
        }

        // ***************** Returning different results from sequential calls - End **********************


        // ***************** Mocking Members of Concrete types with partial mocks & virtual protected members - Start **********************

        [Fact]
        public void ReferFraudRisk()
        {
            Mock<IFrequentFlyerNumberValidator> mockValidator = new Mock<IFrequentFlyerNumberValidator>();
            Mock<FraudLookup> mockFraudLookup = new Mock<FraudLookup>();
            //mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");

            // Mocking Concrete type
            //mockFraudLookup.Setup(x => x.IsFraudRisk(It.IsAny<CreditCardApplication>()))
            //    .Returns(true);

            // mocking protected virtual method
            mockFraudLookup.Protected()
                .Setup<bool>("CheckApplication", ItExpr.IsAny<CreditCardApplication>())
                .Returns(true);

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object, 
                mockFraudLookup.Object);

            var application = new CreditCardApplication();
            CreditCardApplicationDecision decision = sut.Evaluate(application);
            Assert.Equal(CreditCardApplicationDecision.ReferredToHumanFraudRisk, decision);
        }

        // ***************** Mocking Members of Concrete types with partial mocks & virtual protected members - End **********************

        // ***************** Improve Mock Setup Readability with Linq top Mocks - Start ***********************
        [Fact]
        public void LinqToMocks()
        {
            //Mock<IFrequentFlyerNumberValidator> mockValidator =
            //        new Mock<IFrequentFlyerNumberValidator>();
            //mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");
            //mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);
            //var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

            // *** replace fluent style code with LINQ ******
            IFrequentFlyerNumberValidator mockValidator =
                Mock.Of<IFrequentFlyerNumberValidator>
                (
                    validator =>
                    validator.ServiceInformation.License.LicenseKey == "OK" &&
                    validator.IsValid(It.IsAny<string>()) == true
                );
            var sut = new CreditCardApplicationEvaluator(mockValidator);

            var application = new CreditCardApplication { Age = 25 };

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.AutoDeclined, decision);
        }

        // ***************** Improve Mock Setup Readability with Linq top Mocks - End ***********************
    }
}



// *********** Matching Generic type Arguments *****************************

//public interface IDemoInterface
//{
//    bool IsOdd<T>(T number);
//}

//var mock = new Mock<IDemoInterface>();

//// Specific Value (Inferred Generic Type)

//mock.Setup(x => x.IsOdd(1)).Returns(true);
//mock.Object.IsOdd(1); // returns true
//mock.Object.IsOdd(2); // returns false
//mock.Object.IsOdd(1.0); // returns false

//// Any Value for a specific Generic Type

//mock.Setup(x => x.IsOdd(It.IsAny<int>())).Returns(true);
//mock.Object.IsOdd(1); // returns true
//mock.Object.IsOdd(2); // returns true
//mock.Object.IsOdd(1.0); // returns false

//// Any Value for a any Generic Type

//mock.Setup(x => x.IsOdd(It.IsAny<It.IsAnyType>())).Returns(true);
//mock.Object.IsOdd(1); // returns true
//mock.Object.IsOdd(2); // returns true
//mock.Object.IsOdd(1.0); // returns true
//mock.Object.IsOdd("hello"); // returns true

//// Any Generic Type or subtype(derived)

//mock.Setup(x => x.IsOdd(It.IsAny<It.IsSubtype<ApplicationException>>())).Returns(true);
//mock.Object.IsOdd(new ApplicationException()); // returns true
//mock.Object.IsOdd(new Exception()); // returns false
//mock.Object.IsOdd(new WaitHandleCannotBeOpenedException()); // returns true since this class inherits ApplicationException class



// *********** Mocking Async Method Return Values *****************************

//public interface IDemoInterfaceAsync
//{
//    Task StartAsync();
//    Task<int> StopAsync();
//}

//var mock = new Mock<IDemoInterfaceAsync>();

//mock.Setup(x => x.StartAsync()).Returns(Task.CompletedTask);
//mock.Setup(x => x.StopAsync()).Returns(Task.FromResult(42)); or
//mock.Setup(x => x.StopAsync()).ReturnsAsync(42);
