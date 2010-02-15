using System;
using System.Text;
using Microsoft.Silverlight.Testing.Harness;
using Microsoft.Silverlight.Testing.UnitTesting.Harness;
using StatLight.Client.Model.Events;
using StatLight.Core.Reporting.Messages;
using StatLight.Core.Serialization;
using LogMessageType = StatLight.Core.Reporting.Messages.LogMessageType;
using Microsoft.Silverlight.Testing.UnitTesting.Metadata;

namespace StatLight.Client.Harness
{
    internal sealed class ServerHandlingLogProvider : LogProvider
    {
        protected override void ProcessRemainder(LogMessage message)
        {
            var serializedString = message.Serialize();
            StatLightPostbackManager.PostMessage(serializedString);

            //string traceMessage = TraceLogMessage(message).Serialize();
            //StatLightPostbackManager.PostMessage(traceMessage);

            try
            {

                ClientEvent clientEvent;
                if (TryTranslateIntoClientEvent(message, out clientEvent))
                {
                    string clientEventSerialized = clientEvent.Serialize();
                    StatLightPostbackManager.PostMessage(clientEventSerialized);
                }
            }
            catch (Exception ex)
            {
                var messageObject = new MobilOtherMessageType();
                messageObject.Message = ex.ToString();
                messageObject.MessageType = LogMessageType.Error;
                var serializedStringX = messageObject.Serialize();
                StatLightPostbackManager.PostMessage(serializedStringX);
            }
        }

        private static int clientEventOrder = 0;
        private static bool TryTranslateIntoClientEvent(LogMessage message, out ClientEvent clientEvent)
        {
            if (TryGet_InitializationOfUnitTestHarnessClientEvent(message, out clientEvent))
                return true;

            if (TryGet_TestExecutionClassBeginClientEvent(message, out clientEvent))
                return true;

            if (TryGet_TestExecutionClassCompletedClientEvent(message, out clientEvent))
                return true;

            clientEvent = null;

            return false;
        }

        private static bool TryGet_TestExecutionClassBeginClientEvent(LogMessage message, out ClientEvent clientEvent)
        {
            if (message.MessageType == Microsoft.Silverlight.Testing.Harness.LogMessageType.TestExecution)
            {
                if (message.DecoratorMatches(LogDecorator.TestStage, v => (TestStage)v == TestStage.Starting)
                    && message.DecoratorMatches(LogDecorator.TestGranularity, v => (TestGranularity)v == TestGranularity.TestGroup)
                    && message.DecoratorMatches(LogDecorator.NameProperty, v => true)
                    )
                {
                    var name = (string)message.Decorators[LogDecorator.NameProperty];
                    var clientEventX = new TestExecutionClassBeginClientEvent
                    {
                        ClientEventOrder = clientEventOrder++,
                    };
                    ParseClassAndNamespace(name, clientEventX);
                    clientEvent = clientEventX;
                    return true;
                }
            }
            clientEvent = null;
            return false;
        }

        private static bool TryGet_TestExecutionClassCompletedClientEvent(LogMessage message, out ClientEvent clientEvent)
        {
            if (message.MessageType == Microsoft.Silverlight.Testing.Harness.LogMessageType.TestExecution)
            {
                if (message.DecoratorMatches(LogDecorator.TestStage, v => (TestStage)v == TestStage.Finishing)
                    && message.DecoratorMatches(LogDecorator.TestGranularity, v => (TestGranularity)v == TestGranularity.TestGroup)
                    && message.DecoratorMatches(LogDecorator.NameProperty, v => true)
                    )
                {
                    var name = (string)message.Decorators[LogDecorator.NameProperty];
                    var clientEventX = new TestExecutionClassCompletedClientEvent
                    {
                        ClientEventOrder = clientEventOrder++,
                    };
                    ParseClassAndNamespace(name, clientEventX);
                    clientEvent = clientEventX;
                    return true;
                }
            }
            clientEvent = null;
            return false;
        }

        private static void ParseClassAndNamespace(string name, TestExecutionClass e)
        {
            e.ClassName = name.Substring(name.LastIndexOf('.')+1);
            e.NamespaceName = name.Substring(0, name.LastIndexOf('.'));
        }

        private static bool TryGet_InitializationOfUnitTestHarnessClientEvent(LogMessage message, out ClientEvent clientEvent)
        {
            if (message.MessageType == Microsoft.Silverlight.Testing.Harness.LogMessageType.TestInfrastructure)
            {
                if (message.Message.Equals("Initialization of UnitTestHarness", StringComparison.InvariantCultureIgnoreCase))
                {
                    clientEvent = new InitializationOfUnitTestHarnessClientEvent
                                      {
                                          ClientEventOrder = clientEventOrder++,
                                      };
                    return true;
                }
            }
            clientEvent = null;
            return false;
        }

        private TraceClientEvent TraceLogMessage(LogMessage message)
        {
            string msg = "";
            msg += "MessageType={0}".FormatWith(message.MessageType);
            msg += Environment.NewLine;
            msg += "Message={0}".FormatWith(message.Message);
            msg += Environment.NewLine;
            msg += "Decorators:";
            msg += Environment.NewLine;
            msg += GetDecorators(message.Decorators);
            msg += Environment.NewLine;
            return new TraceClientEvent { Message = msg };
        }

        private static string GetDecorators(DecoratorDictionary decorators)
        {
            var sb = new StringBuilder();
            foreach (var k in decorators)
            {
                sb.AppendFormat("KeyType(typeof,string)={0}, {1}, ValueType={2}, {3}{4}",
                    k.Key.GetType(), k.Key,
                    k.Value.GetType(), k.Value, Environment.NewLine);
            }
            return sb.ToString();
        }


        public static void Trace(string message)
        {
            if (string.IsNullOrEmpty(message))
                message = "{StatLight - Trace Message: trace string is NULL or empty}";
            var traceClientEvent = new TraceClientEvent { Message = message };
            string traceMessage = traceClientEvent.Serialize();
            StatLightPostbackManager.PostMessage(traceMessage);
        }
    }

    internal static class UnitTestResultProviderExtensions
    {
        public static string DecoratorDictionaryToString(this DecoratorDictionary d)
        {
            var sb = new StringBuilder();
            foreach (var k in d)
            {
                //if (k.Value.GetType().IsInstanceOfType(typeof(Microsoft.Silverlight.Testing.UnitTesting.Metadata.VisualStudio.TestMethod)))
                //{
                //    sb.AppendFormat("KeyType={0}, ValueType={1}{2}", k.Key, ((Microsoft.Silverlight.Testing.UnitTesting.Metadata.VisualStudio.TestMethod)k.Value).Name, Environment.NewLine);
                //}
                //if (k.Value.GetType().IsInstanceOfType(typeof(Microsoft.Silverlight.Testing.UnitTesting.Metadata.VisualStudio.TestClass)))
                //{
                //    sb.AppendFormat("KeyType={0}, ValueType={1}{2}", k.Key, ((Microsoft.Silverlight.Testing.UnitTesting.Metadata.VisualStudio.TestClass)k.Value).Type.FullName, Environment.NewLine);
                //}
                //else
                sb.AppendFormat("KeyType={0}, ValueType={1}{2}", k.Key, k.Value, Environment.NewLine);
            }
            return sb.ToString();
        }

        public static string Serialize(this LogMessage logMessage)
        {
            if (logMessage.MessageType == Microsoft.Silverlight.Testing.Harness.LogMessageType.TestResult &&
                logMessage.Decorators.ContainsKey(UnitTestLogDecorator.ScenarioResult))
            {
                var scenerioResult = logMessage.Decorators[UnitTestLogDecorator.ScenarioResult] as ScenarioResult;
                var messageObject = new MobilScenarioResult();
                messageObject.ExceptionMessage = scenerioResult.Exception != null ? scenerioResult.Exception.ToString() : string.Empty;
                messageObject.Finished = scenerioResult.Finished;
                messageObject.Result = (StatLight.Core.Reporting.Messages.TestOutcome)scenerioResult.Result;
                messageObject.Started = scenerioResult.Started;
                messageObject.TestClassName = scenerioResult.TestClass.Type.FullName;
                messageObject.TestName = scenerioResult.TestMethod.Name;
                return messageObject.Serialize();
            }
            else if (logMessage.MessageType == (Microsoft.Silverlight.Testing.Harness.LogMessageType)LogMessageType.Error)
            {
                var messageObject = new MobilOtherMessageType();
                messageObject.Message = logMessage.Message + " ---- " + logMessage.Decorators.DecoratorDictionaryToString();
                messageObject.MessageType = (LogMessageType)logMessage.MessageType;
                return messageObject.Serialize();
            }
            else
            {
                var messageObject = new MobilOtherMessageType();
                messageObject.Message = logMessage.Message;
                messageObject.MessageType = (LogMessageType)logMessage.MessageType;
                return messageObject.Serialize();
            }
        }

        public static bool DecoratorMatches(this LogMessage logMessage, object key, Predicate<object> value)
        {
            if (logMessage.Decorators.ContainsKey(key))
            {
                if (value(logMessage.Decorators[key]))
                    return true;
            }
            return false;
        }
    }
}