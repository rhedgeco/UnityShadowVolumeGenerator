// https://stackoverflow.com/questions/58441744/how-to-enable-disable-a-list-in-unity-inspector-using-a-bool

using System;
using UnityEngine;

namespace StencilShadowGenerator.Core.Attributes.DisplayConditional
{
    [AttributeUsage(AttributeTargets.Field)]
    public class DisplayIfAttribute : PropertyAttribute
    {
        public ConditionAction Action { get; private set; }
        public ConditionOperator Operator { get; private set; }
        public string[] Conditions { get; private set; }
        
        public DisplayIfAttribute(params string[] conditions)
        {
            Action = ConditionAction.Hide;
            Operator = ConditionOperator.And;
            Conditions = conditions;
        }
        
        public DisplayIfAttribute(ConditionAction action, params string[] conditions)
        {
            Action = action;
            Operator = ConditionOperator.And;
            Conditions = conditions;
        }
        
        public DisplayIfAttribute(ConditionOperator condition, params string[] conditions)
        {
            Action = ConditionAction.Hide;
            Operator = condition;
            Conditions = conditions;
        }

        public DisplayIfAttribute(ConditionAction action, ConditionOperator condition, params string[] conditions)
        {
            Action = action;
            Operator = condition;
            Conditions = conditions;
        }
    }
}