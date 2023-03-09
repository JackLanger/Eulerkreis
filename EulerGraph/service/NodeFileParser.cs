using System;
using System.Collections.Generic;
using System.Text.Json;
using EulerGraph.graph;

namespace EulerGraph.service;

public class NodeFileParser
{
    public List<Node<T>>? Parse<T>(string content)
    {
        List<Node<T>> res = null;
        try
        {
            res = (List<Node<T>>)JsonSerializer.Deserialize(content, typeof(List<Node<T>>));
        }
        catch (JsonException e)
        {
        }
        catch (InvalidCastException e)
        {
        }

        return res;
    }
}