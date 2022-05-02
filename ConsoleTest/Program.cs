for (var i = 0; i < 20; i++)
    Console.WriteLine($"> {i}");

var ml = new Log.MultiLine();
var l1 = ml.AddLine();
var l2 = ml.AddLine();
var l3 = ml.AddLine("ahoy!");
var l4 = ml.AddLine();
var l5 = ml.AddLine();

Thread.Sleep(1000);
l4.Text = "w00t";
l5.Text = "Last";

Thread.Sleep(1000);
l1.Text = "w00t";

Thread.Sleep(1000);
l4.Text = "w00t000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000008888----";

Thread.Sleep(1000);
l3.Text = "line 1\nline 2";


for(; ; )
{
    l2.Text = "---------------------------------------------------" + DateTime.Now.ToString();
    l5.Text = "Last - " + DateTime.Now.ToString();
    Thread.Sleep(200);
}
