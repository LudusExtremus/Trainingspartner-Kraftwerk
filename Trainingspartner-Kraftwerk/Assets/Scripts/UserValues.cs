using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

public class UserValues : MonoBehaviour {

    public enum FIELD_TYPE
    {
        TYPE_String,
        TYPE_Integer,
        TYPE_Date,
        TYPE_boolean,
        TYPE_ParseUser,
        TYPE_ParseFile,
    };
    public static readonly Type[] USER_FIELD_TYPES = { typeof(string), typeof(int), typeof(DateTime) , typeof(bool)};

    [Serializable]
    public struct Value
    {
        public string name;
        public FIELD_TYPE type;
        public bool isList;
        public bool isProfileRequirement;
        public bool isSortOption;
        public InputField inputField;
    }
    public Value[] customValues;

    public static readonly string TIMETABLE = "timetable";
    public static readonly string NICK = "nick";
    public static readonly string START_DATE = "startDate";
    public static readonly string ABOUT = "about";
    public static readonly string CATEGORIES = "categories";
    public static readonly string PARTNERS = "partners";
    public static readonly string LAST_LOGIN = "lastLogin";
    public static readonly string PICTURE = "picture";
    public static readonly string ACTIVE = "active";

    [Tooltip("Don't edit! (Only linked Input Fields)")]
    public Value[] coreValues = {
        new Value() { name = NICK, type = FIELD_TYPE.TYPE_String, isList = false, isProfileRequirement = true, isSortOption = true},
        new Value() { name = START_DATE, type = FIELD_TYPE.TYPE_String, isList = false, isProfileRequirement = false, isSortOption = true},
        new Value() { name = ABOUT, type = FIELD_TYPE.TYPE_String, isList = false, isProfileRequirement = false, isSortOption = false},
        new Value() { name = CATEGORIES, type = FIELD_TYPE.TYPE_String, isList = true, isProfileRequirement = true, isSortOption = true},
        new Value() { name = PARTNERS, type = FIELD_TYPE.TYPE_ParseUser, isList = true, isProfileRequirement = false, isSortOption = true},
        new Value() { name = LAST_LOGIN, type = FIELD_TYPE.TYPE_Date, isList = false, isProfileRequirement = false, isSortOption = true},
        new Value() { name = PICTURE, type = FIELD_TYPE.TYPE_ParseFile, isList = false, isProfileRequirement = false, isSortOption = true},
        new Value() { name = ACTIVE, type = FIELD_TYPE.TYPE_boolean, isList = false, isProfileRequirement = false, isSortOption = true}
    };


}
