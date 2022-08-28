// See https://aka.ms/new-console-template for more information
using Study;

var recorder = new Recorder();

var list = new List<int>() { 1, 2, 3 };
recorder.Start();
Permutation.Combination1(list, 0, 2, 0, new List<int>());
//Permutation.Combination2(list, 0, 2, 0, new List<int>() {0, 0});
recorder.Stop();
