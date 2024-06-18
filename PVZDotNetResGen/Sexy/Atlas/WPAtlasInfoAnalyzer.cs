using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;
using PVZDotNetResGen.Utils.JsonHelper;
using PVZDotNetResGen.Utils.Sure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Resources;

namespace PVZDotNetResGen.Sexy.Atlas
{
    public static class WPAtlasInfoAnalyzer
    {
        private static void IdLine(StreamReader sr, string line)
        {
            if (sr.ReadLine() != line)
            {
                throw new Exception();
            }
        }

        public static void UnpackAsJson(string csFilePath, string atlasFolderPath)
        {
            var dic = WPAtlasInfoAnalyzer.UnpackAsDictionary(csFilePath);
            WPAtlasInfoAnalyzer.SaveToJson(dic, atlasFolderPath);
        }

        public static void SaveToJson(Dictionary<string, (List<SpriteItem>, string)> dic, string atlasFolderPath)
        {
            foreach (var atlasPair in dic)
            {
                string id = atlasPair.Key;
                string atlasName = atlasPair.Value.Item2;
                List<SpriteItem> items = atlasPair.Value.Item1;
                AOTJson.TrySerializeToFile(Path.Combine(atlasFolderPath, atlasName + ".json"), new AtlasJson { mId = id, mAtlas = items });
            }
        }

        public static Dictionary<string, (List<SpriteItem>, string)> UnpackAsDictionary(string csFilePath)
        {
            Dictionary<string, (List<SpriteItem>, string)> ans = [];
            SyntaxTree tree = CSharpSyntaxTree.ParseText(File.ReadAllText(csFilePath));
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();
            IEnumerable<MethodDeclarationSyntax> methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>();
            foreach (MethodDeclarationSyntax method in methods)
            {
                string methodName = method.Identifier.ToString();
                if (methodName.StartsWith("Unpack") && methodName.EndsWith("AtlasImages"))
                {
                    BlockSyntax? body = method.Body;
                    if (body != null)
                    {
                        List<SpriteItem> items = [];
                        string? atlasName = null;
                        string? atlasId = null;
                        foreach (StatementSyntax statement in body.Statements)
                        {
                            switch (statement)
                            {
                                case LocalDeclarationStatementSyntax localDeclarationStatement:
                                    {
                                        var variableDeclaration = localDeclarationStatement.Declaration;
                                        if (variableDeclaration.Type.ToString() == "UNPACK_INFO[]")
                                        {
                                            // 获取数组元素初始化器
                                            var initializer = variableDeclaration.DescendantNodes().OfType<InitializerExpressionSyntax>().FirstOrDefault();
                                            if (initializer != null)
                                            {
                                                foreach (var expression in initializer.Expressions)
                                                {
                                                    if (expression is ObjectCreationExpressionSyntax objectCreation)
                                                    {
                                                        if (objectCreation.ArgumentList != null)
                                                        {
                                                            var arguments = objectCreation.ArgumentList.Arguments;
                                                            string id = arguments[0].ToString();
                                                            SureHelper.MakeSure(id.StartsWith("AtlasResources."));
                                                            id = id["AtlasResources.".Length..];
                                                            SpriteItem item = new SpriteItem
                                                            {
                                                                mId = id,
                                                                mX = int.Parse(arguments[1].ToString()),
                                                                mY = int.Parse(arguments[2].ToString()),
                                                                mWidth = int.Parse(arguments[3].ToString()),
                                                                mHeight = int.Parse(arguments[4].ToString()),
                                                                mRows = int.Parse(arguments[5].ToString()),
                                                                mCols = int.Parse(arguments[6].ToString()),
                                                                mAnim = Enum.Parse<AnimType>(arguments[7].ToString()["AnimType.AnimType_".Length..]),
                                                                mFrameDelay = int.Parse(arguments[8].ToString()),
                                                                mBeginDelay = int.Parse(arguments[9].ToString()),
                                                                mEndDelay = int.Parse(arguments[10].ToString()),
                                                            };
                                                            items.Add(item);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    break;
                                case ExpressionStatementSyntax expressionStatement:
                                    {
                                        if (expressionStatement.Expression is AssignmentExpressionSyntax assignment
                                                && assignment.Left is ElementAccessExpressionSyntax elementAccessExpression)
                                        {
                                            ArgumentSyntax? argument = elementAccessExpression.ArgumentList.Arguments.FirstOrDefault();
                                            if (argument != null && argument.Expression is LiteralExpressionSyntax literalExpression)
                                            {
                                                atlasName = literalExpression.ToString().Trim('"');
                                            }
                                        }
                                    }
                                    break;
                                case ForStatementSyntax forStatement:
                                    {
                                        var assignments = forStatement.DescendantNodes().OfType<AssignmentExpressionSyntax>().ToList();
                                        foreach (var assignment in assignments)
                                        {
                                            var right = assignment.Right;
                                            if (right is ObjectCreationExpressionSyntax objectCreation && objectCreation.Type.ToString() == "Image" && objectCreation.ArgumentList != null)
                                            {
                                                var arguments = objectCreation.ArgumentList.Arguments;
                                                string name = arguments[0].ToString();
                                                if (name.StartsWith("Resources."))
                                                {
                                                    atlasId = name["Resources.".Length..];
                                                }
                                            }
                                        }
                                    }
                                    break;
                            }
                        }
                        ArgumentException.ThrowIfNullOrEmpty(atlasId, nameof(atlasId));
                        ArgumentException.ThrowIfNullOrEmpty(atlasName, nameof(atlasName));
                        ans.Add(atlasId, (items, atlasName));
                    }
                }
            }
            return ans;
        }

        public static Dictionary<string, (List<SpriteItem>, string)> UnpackAsDictionaryOld(string csFilePath)
        {
            Dictionary<string, (List<SpriteItem>, string)> ans = [];
            using (Stream stream = File.OpenRead(csFilePath))
            {
                using (StreamReader sr = new StreamReader(stream))
                {
                    while (!sr.EndOfStream)
                    {
                        string? line = sr.ReadLine();
                        if (line != null)
                        {
                            if (line.StartsWith("        public override void Unpack") && line.EndsWith("AtlasImages()"))
                            {
                                string atlasName = string.Empty;
                                List<SpriteItem> items = new List<SpriteItem>();
                                IdLine(sr, "        {");
                                IdLine(sr, "            UNPACK_INFO[] array = new UNPACK_INFO[]");
                                IdLine(sr, "            {");
                                string? nextLine;
                                while ((nextLine = sr.ReadLine()) != "            };" && nextLine != null)
                                {
                                    if (nextLine.StartsWith("            new UNPACK_INFO(AtlasResources."))
                                    {
                                        string data = nextLine["            new UNPACK_INFO(AtlasResources.".Length..];
                                        string[] unpack = data.Replace(", ", ",").Split(",");
                                        SpriteItem item = new()
                                        {
                                            mId = unpack[0],
                                            mX = int.Parse(unpack[1]),
                                            mY = int.Parse(unpack[2]),
                                            mWidth = int.Parse(unpack[3]),
                                            mHeight = int.Parse(unpack[4]),
                                            mRows = int.Parse(unpack[5]),
                                            mCols = int.Parse(unpack[6]),
                                            mAnim = Enum.Parse<AnimType>(unpack[7]["AnimType.AnimType_".Length..]),
                                            mFrameDelay = int.Parse(unpack[8]),
                                            mBeginDelay = int.Parse(unpack[9]),
                                            mEndDelay = int.Parse(unpack[10][..unpack[10].IndexOf(')')]),
                                        };
                                        items.Add(item);
                                    }
                                    else
                                    {
                                        throw new Exception(nextLine);
                                    }
                                }
                                int startIndex;
                                nextLine = sr.ReadLine();
                                if (nextLine != null && (startIndex = nextLine.IndexOf("            mArrays[\"")) != -1)
                                {
                                    nextLine = nextLine[(startIndex + "            mArrays[\"".Length)..];
                                    startIndex = nextLine.IndexOf('"');
                                    atlasName = nextLine[..startIndex];
                                }
                                nextLine = sr.ReadLine();
                                nextLine = sr.ReadLine();
                                nextLine = sr.ReadLine();
                                if (nextLine != null && (startIndex = nextLine.IndexOf("new Image(Resources.")) != -1)
                                {
                                    nextLine = nextLine[(startIndex + "new Image(Resources.".Length)..];
                                    startIndex = nextLine.IndexOf(',');
                                    nextLine = nextLine[..startIndex];
                                    ans.Add(nextLine, (items, atlasName));
                                }
                                else
                                {
                                    throw new Exception(nextLine);
                                }
                            }
                        }
                    }
                }
            }
            return ans;
        }

        public class AtlasJson : IJsonVersionCheckable
        {
            public static uint JsonVersion => 0;

            public required string mId;

            public required List<SpriteItem> mAtlas;
        }
    }
}
