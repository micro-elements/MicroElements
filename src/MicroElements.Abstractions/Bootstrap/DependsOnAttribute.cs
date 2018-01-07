// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace MicroElements.Bootstrap
{
    public class DependsOnAttribute : Attribute
    {
        private string _buildStepName;

        public DependsOnAttribute(string buildStepName)
        {
            _buildStepName = buildStepName;
        }
    }
}
