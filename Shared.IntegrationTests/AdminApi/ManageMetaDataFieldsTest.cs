using System;
using System.Collections.Generic;
using System.Linq;
using CloudinaryDotNet.Actions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace CloudinaryDotNet.IntegrationTest.AdminApi
{
    public class ManageMetadataFieldsTest : IntegrationTestBase
    {
        private const string dataSourceId1 = "external_id_1";
        private const string dataSourceId2 = "external_id_2";
        private const string dataSourceValue1 = "blue";
        private const string dataSourceValue2 = "yellow";
        private const string beforeUpdateSuffix = "before_update";
        private const string afterUpdateSuffix = "after_update";
        private const int defaultIntValue = 100;
        private const string defaultStringValue = "some value";
        private readonly DateTime defaultDateTimeValue = new DateTime(2019, 11, 5);

        [Test]
        public void TestAddIntMetadataField()
        {
            var parameters = CreateMetadataFieldParameters<IntMetadataFieldCreateParams, int?>(defaultIntValue);

            var result = m_cloudinary.AddMetadataField(parameters);

            CheckFieldCreationResponse(result, parameters, MetadataFieldType.Integer);
            Assert.AreEqual(parameters.DefaultValue, result.DefaultValue);
        }

        [Test]
        public void TestAddIntMetadataFieldWithValidation()
        {
            var parameters = CreateMetadataFieldParameters<IntMetadataFieldCreateParams, int?>(defaultIntValue);
            var lessThanValidation = new IntLessThanValidationParams(300);
            var greaterThanValidation = new IntGreaterThanValidationParams(50);
            var validationRules = new List<MetadataValidationParams>
            {
                lessThanValidation,
                greaterThanValidation
            };
            parameters.Validation = new AndValidationParams(validationRules);

            var result = m_cloudinary.AddMetadataField(parameters);


            var expectedValidationTypes = new List<MetadataValidationType>
            {
                MetadataValidationType.LessThan, MetadataValidationType.GreaterThan
            };
            CheckFieldValidationResponse(result, validationRules.Count, expectedValidationTypes);
        }

        [Test]
        public void TestAddStringMetadataField()
        {
            var parameters = CreateMetadataFieldParameters<StringMetadataFieldCreateParams, string>(defaultStringValue);

            var result = m_cloudinary.AddMetadataField(parameters);

            CheckFieldCreationResponse(result, parameters, MetadataFieldType.String);
            Assert.AreEqual(parameters.DefaultValue, result.DefaultValue);
        }

        [Test]
        public void TestAddStringMetadataFieldWithValidation()
        {
            var parameters = CreateMetadataFieldParameters<StringMetadataFieldCreateParams, string>(defaultStringValue);
            var strLenValidation = new StringLengthValidationParams
            {
                Min = 5,
                Max = 300
            };
            var validationRules = new List<MetadataValidationParams>
            {
                strLenValidation
            };
            parameters.Validation = new AndValidationParams(validationRules);

            var result = m_cloudinary.AddMetadataField(parameters);

            var expectedValidationTypes = new List<MetadataValidationType>
            {
                MetadataValidationType.StringLength
            };
            CheckFieldValidationResponse(result, validationRules.Count, expectedValidationTypes);
        }

        [Test]
        public void TestAddDateMetadataField()
        {
            var parameters = CreateMetadataFieldParameters<DateMetadataFieldCreateParams, DateTime?>(defaultDateTimeValue);

            var result = m_cloudinary.AddMetadataField(parameters);

            CheckFieldCreationResponse(result, parameters, MetadataFieldType.Date);
            Assert.AreEqual("2019-11-05", result.DefaultValue);
        }

        [Test]
        public void TestAddDateMetadataFieldWithValidation()
        {
            var parameters = CreateMetadataFieldParameters<DateMetadataFieldCreateParams, DateTime?>(defaultDateTimeValue);
            var lessThanValidation = new DateLessThanValidationParams(new DateTime(2020, 1, 1));
            var greaterThanValidation = new DateGreaterThanValidationParams(new DateTime(2001, 1, 1));
            var validationRules = new List<MetadataValidationParams>
            {
                lessThanValidation,
                greaterThanValidation
            };
            parameters.Validation = new AndValidationParams(validationRules);

            var result = m_cloudinary.AddMetadataField(parameters);

            var expectedValidationTypes = new List<MetadataValidationType>
            {
                MetadataValidationType.LessThan, MetadataValidationType.GreaterThan
            };
            CheckFieldValidationResponse(result, validationRules.Count, expectedValidationTypes);
        }

        [Test]
        public void TestAddEnumMetadataField()
        {
            var parameters = CreateMetadataFieldParametersWithDataSource<EnumMetadataFieldCreateParams, 
                string>(dataSourceId1);

            var result = m_cloudinary.AddMetadataField(parameters);

            CheckFieldCreationResponse(result, parameters, MetadataFieldType.Enum);
            Assert.AreEqual(parameters.DataSource.Values[0].ExternalId, result.DefaultValue);
        }

        [Test]
        public void TestAddSetMetadataField()
        {
            var defaultValue = new List<string> { dataSourceId1, dataSourceId2 };
            var parameters = CreateMetadataFieldParametersWithDataSource<SetMetadataFieldCreateParams, 
                List<string>>(defaultValue);

            var result = m_cloudinary.AddMetadataField(parameters);

            CheckFieldCreationResponse(result, parameters, MetadataFieldType.Set);
            var defaultValueAsList = ((JArray)result.DefaultValue).ToObject<List<string>>();
            Assert.AreEqual(parameters.DefaultValue, defaultValueAsList);
        }

        [Test]
        public void TestListMetadataFields()
        {
            var intParams = CreateMetadataFieldParameters<IntMetadataFieldCreateParams, int?>(defaultIntValue);
            var field1 = m_cloudinary.AddMetadataField(intParams);
            var strParams = CreateMetadataFieldParameters<StringMetadataFieldCreateParams, string>(defaultStringValue);
            var field2 = m_cloudinary.AddMetadataField(strParams);

            var result = m_cloudinary.ListMetadataFields();

            Assert.NotNull(result);
            Assert.NotNull(result.MetadataFields);
            var foundExternalIds = result.MetadataFields.Select(field => field.ExternalId).ToList();
            Assert.Contains(field1.ExternalId, foundExternalIds);
            Assert.Contains(field2.ExternalId, foundExternalIds);
        }

        [Test]
        public void TestGetMetadataField()
        {
            var fieldParams = CreateMetadataFieldParameters<IntMetadataFieldCreateParams, int?>(defaultIntValue);
            m_cloudinary.AddMetadataField(fieldParams);

            var result = m_cloudinary.GetMetadataField(fieldParams.ExternalId);

            Assert.NotNull(result);
            Assert.AreEqual(Api.GetCloudinaryParam(MetadataFieldType.Integer), result.Type);
            Assert.AreEqual(fieldParams.ExternalId, result.ExternalId);
            Assert.AreEqual(fieldParams.Label, result.Label);
            Assert.AreEqual(fieldParams.Mandatory, result.Mandatory);
            Assert.AreEqual(fieldParams.DefaultValue, result.DefaultValue);
            Assert.AreEqual(fieldParams.Validation, result.Validation);
        }

        [Test]
        public void TestUpdateIntMetadataField()
        {
            var createParams = CreateMetadataFieldParameters<IntMetadataFieldCreateParams, int?>(
                defaultIntValue, beforeUpdateSuffix);
            m_cloudinary.AddMetadataField(createParams);

            var fieldId = createParams.ExternalId;
            var newLabel = GetUniqueMetadataFieldLabel(afterUpdateSuffix);
            const int newDefaultValue = 200;
            var updateParams = new IntMetadataFieldUpdateParams
            {
                Label = newLabel,
                DefaultValue = newDefaultValue
            };

            var updateResult = m_cloudinary.UpdateMetadataField(fieldId, updateParams);

            CheckFieldUpdateResponse(updateResult, newLabel, fieldId);
            Assert.AreEqual(newDefaultValue, updateResult.DefaultValue);
        }

        [Test]
        public void TestUpdateStringMetadataField()
        {
            var createParams = CreateMetadataFieldParameters<StringMetadataFieldCreateParams, string>(
                defaultStringValue, beforeUpdateSuffix);
            m_cloudinary.AddMetadataField(createParams);

            var fieldId = createParams.ExternalId;
            var newLabel = GetUniqueMetadataFieldLabel(afterUpdateSuffix);
            const string newDefaultValue = "new value";
            var updateParams = new StringMetadataFieldUpdateParams
            {
                Label = newLabel,
                DefaultValue = newDefaultValue
            };

            var updateResult = m_cloudinary.UpdateMetadataField(fieldId, updateParams);

            CheckFieldUpdateResponse(updateResult, newLabel, fieldId);
            Assert.AreEqual(newDefaultValue, updateResult.DefaultValue);
        }

        [Test]
        public void TestUpdateDateMetadataField()
        {
            var createParams = CreateMetadataFieldParameters<DateMetadataFieldCreateParams, DateTime?>(
                defaultDateTimeValue, beforeUpdateSuffix);
            m_cloudinary.AddMetadataField(createParams);

            var fieldId = createParams.ExternalId;
            var newLabel = GetUniqueMetadataFieldLabel(afterUpdateSuffix);
            var newDefaultValue = new DateTime(2019, 11, 7);
            var updateParams = new DateMetadataFieldUpdateParams
            {
                Label = newLabel,
                DefaultValue = newDefaultValue
            };

            var updateResult = m_cloudinary.UpdateMetadataField(fieldId, updateParams);

            CheckFieldUpdateResponse(updateResult, newLabel, fieldId);
            Assert.AreEqual("2019-11-07", updateResult.DefaultValue);
        }

        [Test]
        public void TestUpdateEnumMetadataField()
        {
            var createParams = CreateMetadataFieldParametersWithDataSource<EnumMetadataFieldCreateParams, 
                string>(dataSourceId1);
            m_cloudinary.AddMetadataField(createParams);

            var fieldId = createParams.ExternalId;
            var newLabel = GetUniqueMetadataFieldLabel(afterUpdateSuffix);
            var newDefaultValue = createParams.DataSource.Values[1].ExternalId;
            var updateParams = new EnumMetadataFieldUpdateParams
            {
                Label = newLabel,
                DefaultValue = newDefaultValue
            };

            var updateResult = m_cloudinary.UpdateMetadataField(fieldId, updateParams);

            CheckFieldUpdateResponse(updateResult, newLabel, fieldId);
            Assert.AreEqual(newDefaultValue, updateResult.DefaultValue);
        }

        [Test]
        public void TestUpdateSetMetadataField()
        {
            var defaultValue = new List<string> { dataSourceId1, dataSourceId2 };
            var createParams = CreateMetadataFieldParametersWithDataSource<SetMetadataFieldCreateParams, List<string>>(
                defaultValue, beforeUpdateSuffix);
            m_cloudinary.AddMetadataField(createParams);

            var fieldId = createParams.ExternalId;
            var newLabel = GetUniqueMetadataFieldLabel(afterUpdateSuffix);
            var newDefaultValue = new List<string> {dataSourceId1};
            var updateParams = new SetMetadataFieldUpdateParams
            {
                Label = newLabel,
                DefaultValue = newDefaultValue
            };

            var updateResult = m_cloudinary.UpdateMetadataField(fieldId, updateParams);

            CheckFieldUpdateResponse(updateResult, newLabel, fieldId);
            var defaultValueAsList = ((JArray)updateResult.DefaultValue).ToObject<List<string>>();
            Assert.AreEqual(newDefaultValue, defaultValueAsList);
        }

        [Test]
        public void TestUpdateMetadataDataSourceEntries()
        {
            var parameters = CreateMetadataFieldParametersWithDataSource<EnumMetadataFieldCreateParams, string>(
                dataSourceId1, beforeUpdateSuffix);
            m_cloudinary.AddMetadataField(parameters);

            const string newLabel = "green";
            const string updatedLabel = "gold";
            var updatedEntries = new List<EntryParams>
            {
                new EntryParams(newLabel),
                new EntryParams(updatedLabel, dataSourceId1)
            };
            var updateParams = new MetadataDataSourceParams(updatedEntries);

            Assert.Throws<ArgumentNullException>(() => m_cloudinary.UpdateMetadataDataSourceEntries(null, null));

            var fieldId = parameters.ExternalId;
            var updateResult = m_cloudinary.UpdateMetadataDataSourceEntries(fieldId, updateParams);
            Assert.NotNull(updateResult);
            Assert.AreEqual(3, updateResult.Values.Count);
            Assert.True(updateResult.Values.Any(entry => entry.Value == newLabel));
            Assert.True(updateResult.Values.Any(entry => entry.Value == updatedLabel && entry.ExternalId == dataSourceId1));
        }

        [Test]
        public void TestDeleteMetadataField()
        {
            var fieldParams = CreateMetadataFieldParameters<IntMetadataFieldCreateParams, int?>(defaultIntValue);
            m_cloudinary.AddMetadataField(fieldParams);

            Assert.Throws<ArgumentNullException>(() => m_cloudinary.DeleteMetadataField(null));

            var delResult = m_cloudinary.DeleteMetadataField(fieldParams.ExternalId);
            Assert.NotNull(delResult);
            Assert.AreEqual("ok", delResult.Message);
        }

        [Test]
        public void TestDeleteMetadataDataSourceEntries()
        {
            var parameters = CreateMetadataFieldParametersWithDataSource<EnumMetadataFieldCreateParams, string>(
                dataSourceId1, "del_data_source");
            m_cloudinary.AddMetadataField(parameters);
            var fieldId = parameters.ExternalId;
            var dataSourceId = parameters.DataSource.Values[1].ExternalId;
            var dataSourceIds = new List<string>
            {
                dataSourceId
            };
            Assert.Throws<ArgumentNullException>(() => m_cloudinary.DeleteMetadataDataSourceEntries(null, null));

            var delResult = m_cloudinary.DeleteMetadataDataSourceEntries(fieldId, dataSourceIds);
            Assert.NotNull(delResult);
            Assert.AreEqual(1, delResult.Values.Count);

            var metadataField = m_cloudinary.GetMetadataField(fieldId);
            var deactivatedDataSource =
                metadataField.DataSource.Values.FirstOrDefault(ds => ds.ExternalId == dataSourceId);
            Assert.NotNull(deactivatedDataSource);
            Assert.AreEqual("inactive", deactivatedDataSource.State);
        }

        private static void CheckFieldCreationResponse<T>(
            MetadataFieldResult response,
            MetadataFieldCreateParams<T> requestParameters,
            MetadataFieldType fieldType)
        {
            Assert.NotNull(response);
            Assert.AreEqual(requestParameters.ExternalId, response.ExternalId);
            Assert.AreEqual(Api.GetCloudinaryParam(fieldType), response.Type);
            Assert.AreEqual(requestParameters.Label, response.Label);
            Assert.IsTrue(response.Mandatory);
            Assert.IsNull(response.Validation);
        }

        private static void CheckFieldUpdateResponse(MetadataFieldResult response, string label, string externalId)
        {
            Assert.NotNull(response);
            Assert.AreEqual(externalId, response.ExternalId);
            Assert.AreEqual(label, response.Label);
        }

        private static void CheckFieldValidationResponse(
            MetadataFieldResult response,
            int expectedRulesCount,
            List<MetadataValidationType> expectedValidationTypes)
        {
            Assert.NotNull(response);

            var validationResult = response.Validation;
            Assert.IsNotNull(validationResult);
            Assert.AreEqual(expectedRulesCount, validationResult.Rules.Count);
            Assert.AreEqual(Api.GetCloudinaryParam(MetadataValidationType.And), validationResult.Type);

            var validationTypes = expectedValidationTypes.Select(Api.GetCloudinaryParam).ToList();
            CollectionAssert.AreEquivalent(validationTypes, validationResult.Rules.Select(rule => rule.Type).ToList());
        }

        private T CreateMetadataFieldParametersWithDataSource<T, TP>(TP defaultValue, string suffix = "")
            where T : MetadataFieldCreateParams<TP>
        {
            var parameters = CreateMetadataFieldParameters<T, TP>(defaultValue, suffix);
            var entries = new List<EntryParams>
            {
                new EntryParams(dataSourceValue1, dataSourceId1),
                new EntryParams(dataSourceValue2, dataSourceId2)
            };
            parameters.DataSource = new MetadataDataSourceParams(entries);
            return parameters;
        }

        private T CreateMetadataFieldParameters <T, TP>(TP defaultValue, string suffix = "") 
            where T: MetadataFieldCreateParams<TP>
        {
            var externalId = GetUniqueMetadataFieldExternalId(suffix);
            var label = GetUniqueMetadataFieldLabel(suffix);
            var parameters = (T)Activator.CreateInstance(typeof(T), label);
            parameters.ExternalId = externalId;
            parameters.Mandatory = true;
            parameters.DefaultValue = defaultValue;
            return parameters;
        }

        private string GetUniqueMetadataFieldExternalId(string suffix = "")
        {
            var externalId = $"{m_apiTest}_meta_data_field_{m_metadataFieldsToClear.Count + 1}";
            if (!string.IsNullOrEmpty(suffix))
                externalId = $"{externalId}_{suffix}";

            m_metadataFieldsToClear.Add(externalId);
            return externalId;
        }
    }
}