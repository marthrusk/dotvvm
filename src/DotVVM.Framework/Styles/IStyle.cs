﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Styles
{
    public interface IStyle
    {
        bool Matches(StyleMatchingInfo matcher);
        IStyleApplicator Applicator { get; }
        Type ControlType { get; }
        bool ExactTypeMatch { get; }
    }
}