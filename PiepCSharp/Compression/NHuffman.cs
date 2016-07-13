using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compression
{
    public class Node
    {
        public int frequency;
        public string data;
        public Node leftChild, rightChild;

        public Node(string data,int frequency)
        {
            this.data = data;
            this.frequency = frequency;
        }

        public Node(Node leftChild,Node rightChild)
        {
            this.leftChild = leftChild;
            this.rightChild = rightChild;

            this.data = leftChild.data + ":" + rightChild;
            this.frequency = leftChild.frequency + rightChild.frequency;
        }
    }

    public class NHuffman
    {
        public NHuffman()
        {

        }

        public void startHuffman()
        {
            IList<Node> list = new List<Node>();
            int[] array = new int[] { 2,2,2,3,3,3,3,4,4,4,5,5,5,5,5,5};
            for (int i = 0; i < array.Length; i++)
            {
                list.Add(new Node((i + 1).ToString(), array[i]));
            }

            Stack<Node> stack = getSortedStack(list);

            while (stack.Count>1)
            {
                Node leftChild = stack.Pop();
                Node rightChild = stack.Pop();

                Node parentNode = new Node(leftChild, rightChild);

                stack.Push(parentNode);

                stack = getSortedStack(stack.ToList<Node>());
            }

            Node parentNode1 = stack.Pop();
            GenerateCode(parentNode1, "");
            //DecodeData(parentNode1, parentNode1, 0, "100");

        }


        public Stack<Node> getSortedStack(IList<Node> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                for (int j = 0; j < list.Count; j++)
                {
                    if (list[i].frequency>list[j].frequency)
                    {
                        Node tempNode = list[j];
                        list[j] = list[i];
                        list[i] = tempNode;
                    }
                }
            }

            Stack<Node> stack = new Stack<Node>();
            for (int j = 0; j < list.Count; j++)
            {
                stack.Push(list[j]);
            }
            return stack;
        }

        public void GenerateCode(Node parentNode,string Code)
        {
            if (parentNode!=null)
            {
                GenerateCode(parentNode.leftChild, Code + "0");
                if (parentNode.leftChild==null && parentNode.rightChild==null)
                {
                    Console.WriteLine(parentNode.data + "{" + Code + "}");
                }
                GenerateCode(parentNode.rightChild, Code + "1");
            }
        }

        public void DecodeData(Node parentNode,Node currentNode,int pointer,string input)
        {
            if (input.Length==pointer)
            {
                if (currentNode.leftChild==null&&currentNode.rightChild==null)
                {
                    Console.WriteLine(currentNode.data);
                }
                return;
            }else
            {
                if (currentNode.leftChild==null &&currentNode.rightChild==null)
                {
                    Console.WriteLine(currentNode.data);
                    DecodeData(parentNode, parentNode, pointer, input);
                }else
                {
                    if (input.Substring(pointer,1)=="0")
                    {
                        DecodeData(parentNode, currentNode.leftChild, ++pointer, input);
                    }else
                    {
                        DecodeData(parentNode, currentNode.rightChild, ++pointer, input);
                    }
                }
            }
        }

    }
}
