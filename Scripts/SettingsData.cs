using System;
using System.Collections.Generic;
using UnityEngine;

public class SettingsData{
    public ServiceType service_type;
    public Dictionary<string, object> parameters;

    public static string ToJson(SettingsData settings){
        return string.Format("{{\"service_type\":{0},\"parameters\":{1}}}",
                             (int)settings.service_type,
                             DictionaryToJson(settings.parameters));
    }

    public static SettingsData FromJson(string json) {
        var settings = new SettingsData();
        int startIndex = json.IndexOf("\"service_type\":");
        if (startIndex != -1) {
            startIndex += "\"service_type\":".Length;
            int endIndex = json.IndexOf(",\"parameters\":");
            if (endIndex == -1) {
                endIndex = json.Length - 1;
            }
            string serviceTypeString = json.Substring(startIndex, endIndex - startIndex);
            settings.service_type = (ServiceType)int.Parse(serviceTypeString);
        }
        startIndex = json.IndexOf(",\"parameters\":");
        if (startIndex != -1) {
            startIndex += "\"parameters\":".Length;
            int endIndex = json.Length - 1;
            string parametersString = json.Substring(startIndex + 1, endIndex - startIndex - 1);
            settings.parameters = DictionaryFromJson(parametersString);
        }
        return settings;
    }

    public static string DictionaryToJson(Dictionary<string, object> dictionary){
        if (dictionary == null){
            return "{}";
        }
        var entries = new List<string>();
        foreach (var kvp in dictionary){
            string valueJson;
            if (kvp.Value is Dictionary<string, object>){
                valueJson = DictionaryToJson((Dictionary<string, object>)kvp.Value);
            } 
            else{
                valueJson = "\"" + kvp.Value.ToString() + "\"";
            }
            entries.Add(string.Format("\"{0}\":{1}", kvp.Key, valueJson));
        }
        return "{" + string.Join(",", entries) + "}";
    }

    public static Dictionary<string, object> DictionaryFromJson(string json){
        if (string.Equals(json, "{}", StringComparison.InvariantCulture)) {
            return null;
        }
        var dictionary = new Dictionary<string, object>();
        int startIndex = 1;
        while (startIndex < json.Length - 1){
            int keyStartIndex = json.IndexOf("\"", startIndex) + 1;
            int keyEndIndex = json.IndexOf("\":", keyStartIndex);
            if (keyEndIndex == -1) {
                throw new FormatException("The JSON string is not properly formatted. Could not find end of key.");
            }
            string key = json.Substring(keyStartIndex, keyEndIndex - keyStartIndex);
            Debug.Log(key);
            int valueStartIndex = keyEndIndex + 2;
            int valueEndIndex = json.Length - 1;
            for (int i = valueStartIndex; i < json.Length; i++){
                char c = json[i];
                if (c == '{'){
                    valueEndIndex = i;
                    break;
                }
                else if (c == ','){
                    valueEndIndex = i - 1;
                    break;
                }
                else if (c == '"'){
                    int nextQuoteIndex = json.IndexOf("\"", i + 1);
                    if (nextQuoteIndex == -1){
                        throw new FormatException("The JSON string is not properly formatted. Could not find end of value.");
                    }
                    if (nextQuoteIndex > valueEndIndex){
                        valueEndIndex = nextQuoteIndex;
                    }
                    i = nextQuoteIndex;
                }
            }

            if (valueStartIndex > valueEndIndex){
                throw new FormatException("The JSON string is not properly formatted. Could not find value.");
            }

            string value = json.Substring(valueStartIndex + 1, valueEndIndex - valueStartIndex - 2);
            if (value.StartsWith("{")){
                dictionary[key] = DictionaryFromJson(value);
            }
            else{
                dictionary[key] = value;
            }
            startIndex = valueEndIndex + 2;
        }
        return dictionary;
    }
}