using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class TypewriterStorage
{
    private static readonly StringBuilder textBuffer = new StringBuilder(1024);
    private static readonly StringBuilder tagBuffer = new StringBuilder(32);
    private static readonly Stack<RichTextTag> tagStack = new Stack<RichTextTag>();

    private string content;
    private float startTime = 0;
    private float duration = 0; // 打字机效果时间 平均到每个字符 0为立即显示
    private float endTime = 0;
    private float interval;

    public float EndTime => endTime;
    public float Interval => parseRichTextSuccess ? interval : 0;
    public int NoTagLength => parseRichTextSuccess ? noTagCharList.Count : 0;

    // 富文本标签记录
    private class RichTextTag
    {
        public int startIndex;
        public int endIndex;
        public string startTag;
        public string endTag;
        public bool inStack;
    }

    // 缓存对应长度的字符串
    private Dictionary<int, string> cachedStringDic = new Dictionary<int, string>();
    private List<RichTextTag> tagList = new List<RichTextTag>();
    private List<int> indexList = new List<int>();
    private List<char> noTagCharList = new List<char>();
    private bool parseRichTextSuccess;

    public void SetUp(string content, float startTime, float duration, float endTime = 0)
    {
        Clear();
        this.content = content;
        this.duration = duration;
        this.startTime = startTime;
        this.endTime = startTime + (endTime > 0 ? endTime : duration);
        parseRichTextSuccess = ParseRichText(content);
        interval = this.duration / Mathf.Max(1, noTagCharList.Count);
    }

    public void Clear()
    {
        content = null;
        duration = 0;
        endTime = 0;
        interval = 0;
        parseRichTextSuccess = false;
        cachedStringDic.Clear();
        tagList.Clear();
        indexList.Clear();
        noTagCharList.Clear();
    }

    public string GetString(int index)
    {
        return GetString(index * interval + startTime);
    }

    public string GetString(float currentTime)
    {
        // 富文本解析失败 直接退出
        if (!parseRichTextSuccess)
            return string.Empty;

        // 打印时间小于等于0 直接显示
        if (duration <= 0 || currentTime >= endTime)
            return content;

        int currentIndex = (int)((currentTime - startTime) / interval);
        if (cachedStringDic.ContainsKey(currentIndex))
            return cachedStringDic[currentIndex];

        if (currentIndex >= indexList.Count)
            return content;

        string result = ConstructFullString(currentIndex + 1);
        cachedStringDic[currentIndex] = result;
        return result;
    }

    private string ConstructFullString(int length)
    {
        tagStack.Clear();
        textBuffer.Clear();
        int tagListCount = tagList.Count;
        // 遍历到_maxIndex 拼成字符串
        for (int i = 0; i < length; i++)
        {
            int realIndex = indexList[i];
            for (int j = 0; j < tagListCount; j++)
            {
                var tag = tagList[j];

                // 解析富文本确定标签无法交错
                if (realIndex >= tag.startIndex && realIndex < tag.endIndex && !tag.inStack)
                {
                    // 若当前真实索引大于起始索引 小于结束索引
                    // 将startTag加入buffer
                    textBuffer.Append(tag.startTag);
                    // 将tag入栈 并标记
                    tag.inStack = true;
                    tagStack.Push(tag);
                }
                if (realIndex >= tag.endIndex && tag.inStack)
                {
                    // 当索引大于标志结束索引 且该tag仍在栈中
                    // 出栈
                    // 将endTag加入buffer
                    var endTag = tagStack.Pop();
                    endTag.inStack = false;
                    textBuffer.Append(tag.endTag);
                }
            }

            textBuffer.Append(noTagCharList[i]);
        }

        // 当前剩余结束标签直接加入
        while (tagStack.Count > 0)
        {
            var tag = tagStack.Pop();
            tag.inStack = false;
            textBuffer.Append(tag.endTag);
        }

        return textBuffer.ToString();
    }

    private bool ParseRichText(string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            Debug.Log("subtitle is empty or null");
            return false;
        }

        tagStack.Clear();
        tagList.Clear();
        indexList.Clear();
        noTagCharList.Clear();

        for (int i = 0; i < content.Length; i++)
        {
            char c = content[i];
            if (c == '<')
            {
                if (IsStartTag(content, i))
                {
                    string tag = GetTag(content, ref i);
                    tagStack.Push(new RichTextTag()
                    {
                        startIndex = i,
                        startTag = tag
                    });
                }
                else if (IsEndTag(content, i) && tagStack.Count > 0)
                {
                    RichTextTag richTextTag = tagStack.Pop();
                    string startTag = richTextTag.startTag;
                    string endTag = GetTag(content, ref i);
                    if (CompareTag(startTag, endTag))
                    {
                        richTextTag.endIndex = i;
                        richTextTag.endTag = endTag;
                        tagList.Add(richTextTag);
                    }
                    else
                    {
                        Debug.Log($"Tag Mismatch {startTag} & {endTag}");
                        return false;
                    }
                }
                else
                {
                    Debug.Log($"Tag Invalid");
                    return false;
                }
            }
            else
            {
                noTagCharList.Add(c);
                indexList.Add(i);
            }
        }
        return true;
    }

    private bool IsStartTag(string content, int index)
    {
        return index + 1 < content.Length && content[index + 1] != '/';
    }

    private bool IsEndTag(string content, int index)
    {
        return index + 1 < content.Length && content[index + 1] == '/';
    }

    private string GetTag(string content, ref int index)
    {
        tagBuffer.Clear();
        while (content[index] != '>')
        {
            tagBuffer.Append(content[index]);
            index++;
        }

        // 补上右括号
        tagBuffer.Append('>');

        return tagBuffer.ToString();
    }

    private bool CompareTag(string startTag, string endTag)
    {
        for (int i = 1; i < startTag.Length; i++)
        {
            if (startTag[i] == '=')
                break;

            if (i + 1 >= endTag.Length)
                return false;

            if (startTag[i] != endTag[i + 1])
                return false;
        }
        return true;
    }
}
