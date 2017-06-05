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

namespace BlynkLibrary
{
    // This file holdes the enum definitions used within the Blynk Library

    /// <summary>
    /// This enum holds the Blynk command number definitions.
    /// </summary>
    public enum Command : byte
    {
        RESPONSE = 0,

        //app commands
        REGISTER                = 1,
        LOGIN                   = 2,
        REDEEM                  = 3,
        HARDWARE_CONNECTED      = 4,
        GET_TOKEN               = 5,
        PING                    = 6,
        ACTIVATE_DASHBOARD      = 7,
        DEACTIVATE_DASHBOARD    = 8,
        REFRESH_TOKEN           = 9,
        GET_GRAPH_DATA          = 10,
        GET_GRAPH_DATA_RESPONSE = 11,

        //HARDWARE commands
        TWEET               = 12,
        EMAIL               = 13,
        PUSH_NOTIFICATION   = 14,
        BRIDGE              = 15,
        HARDWARE_SYNC       = 16,
        BLYNK_INTERNAL      = 17,
        SMS                 = 18,
        SET_WIDGET_PROPERTY = 19,
        HARDWARE            = 20,

        //app commands
        CREATE_DASH          = 21,
        UPDATE_DASH          = 22,
        DELETE_DASH          = 23,
        LOAD_PROFILE_GZIPPED = 24,
        APP_SYNC             = 25,
        SHARING              = 26,
        ADD_PUSH_TOKEN       = 27,
        EXPORT_GRAPH_DATA    = 28,

        //app sharing commands
        GET_SHARED_DASH     = 29,
        GET_SHARE_TOKEN     = 30,
        REFRESH_SHARE_TOKEN = 31,
        SHARE_LOGIN         = 32,

        //app commands
        CREATE_WIDGET = 33,
        UPDATE_WIDGET = 34,
        DELETE_WIDGET = 35,

        //energy commands
        GET_ENERGY              = 36,
        ADD_ENERGY              = 37,
        UPDATE_PROJECT_SETTINGS = 38,
        ASSIGN_TOKEN            = 39,
        GET_SERVER              = 40,
        CONNECT_REDIRECT        = 41,
        CREATE_DEVICE           = 42,
        UPDATE_DEVICE           = 43,
        DELETE_DEVICE           = 44,
        GET_DEVICES             = 45,
        CREATE_TAG              = 46,
        UPDATE_TAG              = 47,
        DELETE_TAG              = 48,
        GET_TAGS                = 49,
        APP_CONNECTED           = 50,
        UPDATE_FACE             = 51,

        //------------------------------------------

        //web sockets
        WEB_SOCKETS          = 52,
        EVENTOR              = 53,
        WEB_HOOKS            = 54,
        CREATE_APP           = 55,
        UPDATE_APP           = 56,
        DELETE_APP           = 57,
        GET_PROJECT_BY_TOKEN = 58,
        EMAIL_QR             = 59,


        //http codes. Used only for stats
        HTTP_IS_HARDWARE_CONNECTED = 62,
        HTTP_IS_APP_CONNECTED      = 63,
        HTTP_GET_PIN_DATA          = 64,
        HTTP_UPDATE_PIN_DATA       = 65,
        HTTP_NOTIFY                = 66,
        HTTP_EMAIL                 = 67,
        HTTP_GET_PROJECT           = 68,
        HTTP_QR                    = 69,
        HTTP_GET_HISTORY_DATA      = 70,
        HTTP_TOTAL                 = 71,

    }

    /// <summary>
    /// This enum holds the Blynk response code definitions.
    /// </summary>
    public enum Response: int
    {
        OK                            = 200,
        QUOTA_LIMIT                   = 1,
        ILLEGAL_COMMAND               = 2,
        USER_NOT_REGISTERED           = 3,
        USER_ALREADY_REGISTERED       = 4,
        USER_NOT_AUTHENTICATED        = 5,
        NOT_ALLOWED                   = 6,
        DEVICE_NOT_IN_NETWORK         = 7,
        NO_ACTIVE_DASHBOARD           = 8,
        INVALID_TOKEN                 = 9,
        ILLEGAL_COMMAND_BODY          = 11,
        GET_GRAPH_DATA                = 12,
        NOTIFICATION_INVALID_BODY     = 13,
        NOTIFICATION_NOT_AUTHORIZED   = 14,
        NOTIFICATION_ERROR            = 15,
                                          
        BLYNK_TIMEOUT                 = 16,
        NO_DATA                       = 17,
        DEVICE_WENT_OFFLINE           = 18,
        SERVER_ERROR                  = 19,
        NOT_SUPPORTED_VERSION         = 20,
        ENERGY_LIMIT                  = 21,
        FACEBOOK_USER_LOGIN_WITH_PASS = 22
    }
}
