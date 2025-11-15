// See https://aka.ms/new-console-template for more information


using System.IO.Compression;
using ClassLibrary1;
using Microsoft.Extensions.Configuration;
using SVM;
using KernelType = SVM.KernelType;

var svmTargets = Environment.GetEnvironmentVariable("svm_targets") ?? "0";
var targets = svmTargets.Split('-');
var svmSize = Environment.GetEnvironmentVariable("svm_size");
Logger.Log($"Params: {svmSize} {svmTargets}");
var success = Int32.TryParse(svmSize, out int size);
if (!success) size = int.MaxValue;

Logger.Log($"Params: {size} {string.Join(" ", targets)}");
Digits digits = new(targets, size);
Logger.Log("started");
digits.Main();
//await digits.MainLoad();


Logger.Log("end ---------------------------------------------------------------");

