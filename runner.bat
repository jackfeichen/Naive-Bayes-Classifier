FOR %%A IN (1 2 3 4 5 6 7 8 9 10) DO bin\Debug\classifier.exe splits/train%%A splits/test%%A > results/result%%A
