////////////////////////////////////////////////////////////////////////////
//
//  This file is part of BlynkLibrary
//
//  Copyright (c) 2017, Sverre Frøystein
//
//  Permission is hereby granted, free of charge, to any person obtaining a copy of 
//  this software and associated documentation files (the "Software"), to deal in 
//  the Software without restriction, including without limitation the rights to use, 
//  copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the 
//  Software, and to permit persons to whom the Software is furnished to do so, 
//  subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in all 
//  copies or substantial portions of the Software.
//
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
//  INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
//  PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
//  HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION 
//  OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
//  SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;

namespace BlynkLibrary
{
    /// <summary>
    /// This class defines the layout of a virtual pin.
    /// </summary>
    public class VirtualPin
    {
        /// <summary>
        /// This is the pin number.
        /// </summary>
        public int    Pin;
        /// <summary>
        /// This is the pin values. Each element in this list represents one virtual pin parameter
        /// where the parameter index corrsponds with the index within this list.
        /// </summary>
        public List<object> Value = new List<object>(){ 0 };
    }

    /// <summary>
    /// This class represents the event handler argument used by the VirtualPinReceivedEventHandler.
    /// </summary>
    public class VirtualPinEventArgs : EventArgs
    {
        /// <summary>
        /// The virtual pin received.
        /// </summary>
        public VirtualPin Data = new VirtualPin();
    }

    /// <summary>
    /// This class defines the layout of a digital pin.
    /// </summary>
    public class DigitalPin
    {
        /// <summary>
        /// This is the pin number.
        /// </summary>
        public int  Pin;
        /// <summary>
        /// This is the digital pin boolean value. 
        /// </summary>
        public bool Value;
    }

    /// <summary>
    /// This class represents the event handler argument used by the DigitalPinReceivedEventHandler.
    /// </summary>
    public class DigitalPinEventArgs : EventArgs
    {
        /// <summary>
        /// The digital pin received.
        /// </summary>
        public DigitalPin Data = new DigitalPin();
    }
}
