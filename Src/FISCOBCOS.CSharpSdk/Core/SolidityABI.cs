using FISCOBCOS.CSharpSdk.Dto;
using Nethereum.ABI.FunctionEncoding;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.ABI.JsonDeserialisation;
using Nethereum.ABI.Model;
using Nethereum.Contracts;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RPC.Eth.DTOs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FISCOBCOS.CSharpSdk.Core
{
    /// <summary>
    /// 合约解析
    /// </summary>
    public class SolidityABI
    {

        public ContractABI contractAbi;

        public SolidityABI(string abi)
        {
            contractAbi = new ABIDeserialiser().DeserialiseContract(abi);
        }

        /// <summary>
        /// 解析Input
        /// </summary>
        /// <param name="functionName">对应的input 方法名</param>
        /// <param name="encodeInput">原始input</param>
        /// <returns>解析后的合约input参数集合</returns>
        public List<ParameterOutput> InputDecode(string functionName, string encodeInput)
        {
            var function = contractAbi.Functions.FirstOrDefault(e => e.Name == functionName);
            var functionBuilder = new FunctionBuilder("", function);

            string signatureStr = "";
            if (BaseConfig.IsSMCrypt)//国密版本
            {
                var sm3Signature = SolidityABI.GetMethodId(function).ToHex();
                signatureStr = sm3Signature;
                List<ParameterOutput> functionDecode = new FunctionCallDecoder().DecodeFunctionInput(signatureStr, encodeInput, function.InputParameters);
                return functionDecode;
            }
            else
            {
                var sha3Signature = function.Sha3Signature;// "0x53ba0944";
                signatureStr = sha3Signature;
                List<ParameterOutput> functionDecode = functionBuilder.DecodeInput(encodeInput);
                return functionDecode;
            }


        }

        /// <summary>
        /// 解析Input
        /// </summary>
        /// <param name="functionName">对应的input 方法名</param>
        /// <param name="encodeInput">原始input</param>
        /// <returns>解析后的合约input参数集合</returns>
        public FunctionInputDTO InputDecode<FunctionInputDTO>(string functionName, string encodeIutput) where FunctionInputDTO : class, new()
        {

            var functionCallDecoder = new FunctionCallDecoder();
            var function = contractAbi.Functions.FirstOrDefault(e => e.Name == functionName);
            var data = functionCallDecoder.DecodeFunctionInput<FunctionInputDTO>(function.Sha3Signature, encodeIutput);
            return data;

        }

        /// <summary>
        /// 解析output
        /// </summary>
        /// <param name="functionName">对应的output 方法名</param>
        /// <param name="encodeInput">原始output</param>
        /// <returns>解析后的合约output参数集合</returns>
        public List<ParameterOutput> OutputDecode(string functionName, string encodeOutput)
        {
            var function = contractAbi.Functions.FirstOrDefault(e => e.Name == functionName);
            var functionBuilder = new FunctionBuilder("", function);
            List<ParameterOutput> functionDecode = functionBuilder.DecodeOutput(encodeOutput);
            return functionDecode;

        }

        /// <summary>
        /// 解析output
        /// </summary>
        /// <typeparam name="FunctionOutputDTO">解析output Dto</typeparam>
        /// <param name="encodeOutput">原始output</param>
        /// <returns>解析后得到对象</returns>
        public FunctionOutputDTO OutputDecode<FunctionOutputDTO>(FunctionOutputDTO outputDto, string encodeOutput)
        {
            var functionCallDecoder = new FunctionCallDecoder();
            var outputData = functionCallDecoder.DecodeFunctionOutput<FunctionOutputDTO>(outputDto, encodeOutput);
            return outputData;
        }

        /// <summary>
        /// 解析Event
        /// </summary>
        /// <param name="eventName">even名称</param>
        /// <param name="logs">对应logs</param>
        /// <returns>返回解析后的Event数据</returns>
        public List<EventLog<List<ParameterOutput>>> EventDecode(string eventName, BcosFilterLog[] logs)
        {
            var eventAbi = contractAbi.Events.FirstOrDefault(e => e.Name == eventName);
            string signatureStr = "";
            List<FilterLog> tempList = new List<FilterLog>();
            List<EventLog<List<ParameterOutput>>> tempEventLogList = new List<EventLog<List<ParameterOutput>>>();
            if (BaseConfig.IsSMCrypt)//国密版本
            {
                var sm3Signature = SolidityABI.GetEventId(eventAbi).ToHex();
                signatureStr = sm3Signature;
                var jLogs = JArray.FromObject(logs);
                foreach (JToken log in jLogs)
                {

                    FilterLog filterLog = JsonConvert.DeserializeObject<FilterLog>(log.ToString());
                    tempList.Add(filterLog);
                    /* if (EventExtensions.IsLogForEvent(filterLog, signatureStr))
                         {
                             tempList.Add(filterLog);
                         }*/
                }


                foreach (var t in tempList)
                {
                    var tempEventLog = new EventLog<List<ParameterOutput>>(new EventTopicDecoder().DecodeDefaultTopics(eventAbi, t.Topics, t.Data), (FilterLog)t);
                    tempEventLogList.Add(tempEventLog);

                }


                return tempEventLogList;

            }
            else
            {
                /* var sha3Signature = eventAbi.Sha3Signature;// "0x53ba0944";
                 signatureStr = sha3Signature;*/
                var list = EventExtensions.DecodeAllEventsDefaultTopics(eventAbi, JArray.FromObject(logs));
                return list;
            }



        }


        public T EventDecode<T>(string eventName, BcosFilterLog[] logs) where T : class, new()
        {
            var setEventAbi = contractAbi.Events.FirstOrDefault(e => e.Name == eventName);
            ParameterDecoder parameterDecoder = new ParameterDecoder();
            //嵌套查询 指定的event 名称 返回数据必须满足x.Topics.Select(t => t.ToString().Contains(setEventAbi.Sha3Signature)).Any()==true
            var log = logs.Where(x => (x.Topics.Select(t => t.ToString().Contains(setEventAbi.Sha3Signature)).Any())).FirstOrDefault();
            var data = (T)parameterDecoder.DecodeAttributes(log.Data, typeof(T));
            return data;
        }

        public static byte[] Pack(ContractABI abi, string funcName, object[] args, bool sm)
        {
            if (sm)
            {
                Nethereum.ABI.FunctionEncoding.ParametersEncoder pe = new Nethereum.ABI.FunctionEncoding.ParametersEncoder();

                if (string.IsNullOrEmpty(funcName))
                {
                    return pe.EncodeParameters(abi.Constructor.InputParameters, args);
                }
                else
                {
                    byte[] arguments = null, id = null;
                    foreach (var item in abi.Functions)
                    {
                        if (item.Name == funcName)
                        {
                            id = GetMethodId(item);
                            arguments = pe.EncodeParameters(item.InputParameters, args);
                        }
                    }
                    byte[] hash = new byte[arguments.Length + id.Length];
                    Array.Copy(id, 0, hash, 0, id.Length);
                    Array.Copy(arguments, 0, hash, id.Length, arguments.Length);
                    return hash;
                }
            }
            else
            {
                Nethereum.ABI.FunctionEncoding.ParametersEncoder pe = new Nethereum.ABI.FunctionEncoding.ParametersEncoder();

                if (string.IsNullOrEmpty(funcName))
                {
                    return pe.EncodeParameters(abi.Constructor.InputParameters, args);
                }
                else
                {
                    byte[] arguments = null, id = null;
                    foreach (var item in abi.Functions)
                    {
                        if (item.Name == funcName)
                        {
                            id = Hex.Decode(item.Sha3Signature);
                            arguments = pe.EncodeParameters(item.InputParameters, args);
                        }
                    }
                    byte[] hash = new byte[arguments.Length + id.Length];
                    Array.Copy(id, 0, hash, 0, id.Length);
                    Array.Copy(arguments, 0, hash, id.Length, arguments.Length);
                    return hash;
                }
            }
        }

        public static byte[] GetMethodId(FunctionABI func)
        {
            List<Parameter> list = new List<Parameter>(func.InputParameters);
            byte[] hash = DigestUtilities.CalculateDigest
                ("SM3", System.Text.Encoding.UTF8.GetBytes(string.Format("{0}({1})", func.Name,
                string.Join(',', list.ConvertAll(a => a.Type)))));
            byte[] id = new byte[4];
            Array.Copy(hash, id, 4);
            return id;
        }

        public static byte[] GetEventId(EventABI eventABI)
        {
            List<Parameter> list = new List<Parameter>(eventABI.InputParameters);
            byte[] hash = DigestUtilities.CalculateDigest
                ("SM3", System.Text.Encoding.UTF8.GetBytes(string.Format("{0}({1})", eventABI.Name,
                string.Join(',', list.ConvertAll(a => a.Type)))));
            byte[] id = new byte[4];
            Array.Copy(hash, id, 4);
            return id;
        }
    }
}
