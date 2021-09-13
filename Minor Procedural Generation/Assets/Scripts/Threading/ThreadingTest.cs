using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

public class ThreadingTest : MonoBehaviour
{
    static Thread testY = new Thread(writeY); //once they have finished the function, isAlive will return false
    static Thread testX = new Thread(NotDone);
    static readonly object locker = new object();

    bool done = false;
    static bool notDone = false;

    void FirstTest()
    {
        Debug.Log("running!");
        testY.Start();
        testX.Start();
        ThreadingTest tt = new ThreadingTest();
        new Thread(tt.Done).Start();
        tt.Done();
        Debug.Log("Waiting...");  //since this is running on main thread and the testY thread already started, it already prints a few Y's before printing this.
        testY.Join();       //while waiting the thread does not consume any cpu resources
        Debug.Log("TestY is done!");
        Debug.Log("Is it?");
    }

    void SecondTest()
    {
        //threads do not start / or are the same speed / in order, they still can print at different times

        Thread t = new Thread(() => print("Wow this works?!"));    //  ( () = > )  lambda expression
        t.Start();
        Thread t2 = new Thread(() =>
        {
            Debug.Log("Hello!");
            Debug.Log("How are you doing?!");
        });
        t2.Start();

        Thread t3 = new Thread(() =>
        {
            print("Printing!");
            print("I printed?!");
        });
        t3.Start();

        Thread t4 = new Thread(print);
        t4.Start("Bleep bloop");        //prints from system threading, not from debug log, we need to cast in the printer function

        Thread t5 = new Thread(castPrint); //<== changed the method
        t5.Start("Bleep bloop");        //this prints now from unity
    }

    void ThirdTest()
    {
        Task<string> task = Task.Factory.StartNew<string>(() => returnWeirdString("Damn"));

        string result = task.Result;
        Debug.Log(result);
    }

    void Start()
    {
        //FirstTest();
        //SecondTest();
        ThirdTest();

    }

    static string returnWeirdString(string text)
    {
        string message = "Wow what a weird text: " + text;
        return message;
    }

    static void print ( string message)
    {
        Debug.Log(message);
    }

    static void castPrint(object messageObj)
    {
        string message = (string)messageObj;
        Debug.Log(message);
    }


    static void NotDone()
    {
        lock (locker)       //locks this if another thread is reading or writing in this field, this waiting thread does not consume cpu resources
        {
            if (!notDone)
            {
                Debug.Log("notDone!");  //by first writing to console before setting the bool, has high chance of printing twice
                notDone = true;                 //if 2 threads do this method.
            }
        }
    }

    void Done()
    {
        if (!done)
        {
            done = true;
            Debug.Log("Done!");
        }
    }

    static void writeY()
    {
        for (int i = 0; i < 50; i++) Debug.Log("y");
        Thread.Sleep(100); //since its in start function the start will take longer to load. 
        //while sleeping the thread does not consume cpu resources
    }

    static void writeX()
    {
        for (int i = 0; i < 50; i++) Debug.Log("x");
    }

}
