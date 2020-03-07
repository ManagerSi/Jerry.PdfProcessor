using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Jerry.Common.Interface;
using Jerry.Model;
using Newtonsoft.Json;

namespace Jerry.PdfProcessor.Logic.CommandHandle.Impl
{
    public class CommandHandleFactory:ICommandHandleFactory,IEnumerable
    {
        private readonly ILogManager _logManager;
        private readonly ConcurrentDictionary<string, ICommandHandle> _commandHandleDic;

        public CommandHandleFactory(ILogManager logManager)
        {
            _logManager = logManager;
            _commandHandleDic = new ConcurrentDictionary<string, ICommandHandle>();
        }

        public void Add(string commandType, ICommandHandle commandHandle)
        {
            _commandHandleDic.TryAdd(commandType, commandHandle);

            //if (_commandHandleDic.ContainsKey(commandType))
            //    _commandHandleDic.Add(commandType,commandHandle);
        }
        public ICommandHandle CreadCommandHandle(string commandType)
        {
            if (string.IsNullOrEmpty(commandType) || !_commandHandleDic.TryGetValue(commandType,out var commandHandle))
            {
                return new InvalidMessageCommandHandle(_logManager);
            }
            return commandHandle;
        }

        public IEnumerator GetEnumerator()
        {
            return _commandHandleDic.GetEnumerator();
        }
    }
}
