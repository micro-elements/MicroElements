// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace MicroElements.Bootstrap
{
    public interface IBuildStep
    {
        void Execute(BuildContext buildContext);
    }

    public class BuildStepAttribute : Attribute
    {
        private string _name;

        public BuildStepAttribute(string name)
        {
            _name = name;
        }
    }
}
