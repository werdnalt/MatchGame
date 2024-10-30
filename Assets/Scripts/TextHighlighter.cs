using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class TextHighlighter: MonoBehaviour
{
    public static TextHighlighter Instance;
    
    [Serializable]
    public struct Keyword
    {
        public string pattern;
        public string spriteName; 
        public Color highlightColor; 
    }
    
    public List<Keyword> keywords = new List<Keyword>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public string HighlightKeywords(string text)
    {
        // Check if text is null and return an empty string if so
        if (string.IsNullOrEmpty(text))
        {
            Debug.LogError("HighlightKeywords: Input text is null or empty.");
            return string.Empty;
        }

        foreach (var keyword in keywords)
        {
            // Use the pattern directly for regex matching
            var pattern = keyword.pattern;
        
            // Create a color string in hex format
            string colorHex = ColorUtility.ToHtmlStringRGB(keyword.highlightColor);

            // Prepare the sprite insertion if the sprite name is not empty
            var spriteInsertion = string.Empty;
        
            // Assuming the spriteName corresponds to an index in the TMP sprite asset
            if (!string.IsNullOrEmpty(keyword.spriteName))
            {
                // Assuming spriteName is the index number of the sprite in the sprite asset
                spriteInsertion = $"{keyword.spriteName}";
            }

            // Highlight the matches with the specified color and insert sprite
            var replacement = $"{spriteInsertion} <color=#{colorHex}>$&</color>"; // $& represents the entire match
            text = Regex.Replace(text, pattern, replacement);
        }
        return text;
    }
}
