﻿using FlowStateMachine.FlowMessageUnion;
using UnityEngine;

namespace FlowStateMachine.FlowMessages
{
    public sealed class FlowMessageExample1 : FlowMessage
    {
        [SerializeField] private FlowMessageDataExample1 m_message;

        public override FlowMessageData Message
        {
            get
            {
                var message = new FlowMessageData
                {
                    Example1 = m_message
                };
                return message;
            }
        }
    }
}