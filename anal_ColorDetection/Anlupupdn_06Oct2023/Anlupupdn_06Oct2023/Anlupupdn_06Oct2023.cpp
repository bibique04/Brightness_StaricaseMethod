// ConsoleApplication1.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>
// Anlupdn_06Oct2023.cpp : このファイルには 'main' 関数が含まれています。プログラム実行の開始と終了がそこで行われます。
//

#pragma warning (disable: 4996)
#include <iostream>

#include <math.h>
#include <stdio.h>
#include <stdlib.h>
#pragma hdrstop
//#include <condefs.h>
#define pi									 (4.0*atan(1.0))
//---------------------------------------------------------------------------
#pragma argsused
#define NN 1440

int NumColor[12];
int no[4800], Contrast[4800], Color[4800], Detect[4800];
int tmp1, tmp2;
int RsltUpDown[12][NN];
int RsltUpDown2[12][NN];
int RsltUpDown3[12][NN];
int Sum[12];
int cntr;
double PhaseAngle[12] = { 0,30,60,90,120,150, 180, 210, 240, 270 };
double ReversalPoints[12][50];
int cntr2 = 0;

int main(int argc, char** argv)
{

	FILE* fp1, * fp2, * fp3;
	char	filename0[120], filename1[120], filename2[120];
	int i, j, k, indx1, indx2;
	int data_number;
	int Cntr;
	double Mean[12];
	float Correct79;
	double amp, Red, Green, dummy = 0.0;
	char buff[300];
	int tmp;


	if (argc != 2) {
		printf("Parameter Error !\n");
		exit(0);
	}

	//		data_number=atoi(argv[2]);

	sprintf(filename0, "%s.rst", argv[1]);
	sprintf(filename1, "%s.udn", argv[1]);
	sprintf(filename2, "%s.ccs", argv[1]);
	//sprintf(filename0, "TN_LMColor_14Sep2019.rst", argv[1]);
	//sprintf(filename1, "TN_LMColor_14Sep2019.udn", argv[1]);
	//sprintf(filename2, "TN_LMColor_14Sep2019.ccs", argv[1]);

	if (NULL == (fp1 = fopen(filename0, "r"))) {
		printf("file open error! [%s]\07\n", filename0);
		exit(0);
	}
	if (NULL == (fp2 = fopen(filename1, "w"))) {
		printf("file open error! [%s]\07\n", filename1);
		exit(0);
	}
	if (NULL == (fp3 = fopen(filename2, "w"))) {
		printf("file open error! [%s]\07\n", filename2);
		exit(0);
	}
	data_number = 0;
	fgets(buff, 250, fp1);
	while (fscanf(fp1, "%d %d %d %d %d\n",
		&no[data_number], &Contrast[data_number], &Color[data_number], &tmp, &Detect[data_number]) != EOF) {
		//            printf("%d\n",no[data_number]);
		data_number++;
		if (data_number > NN) {
			printf("too many data points !");
			exit(0);
		}
	}
	printf("%d\n", data_number);
	//        for(i=0;i<data_number;i++){
	//              Color[i]=Color[i]/30;
	//        }
	 //   exit(1);
	for (i = 0; i < data_number; i++) {
		/*	printf("%d %d %d\n",no[i],Contrast[i],Color[i]);*/
		indx1 = Color[i];
		indx2 = NumColor[indx1];
		RsltUpDown[indx1][indx2] = Contrast[i];
		NumColor[indx1]++;
	}
	for (j = 0; j < 12; j++) {
		for (i = 1; i < NumColor[indx1]; i++) {
			RsltUpDown2[j][i] = RsltUpDown[j][i] - RsltUpDown[j][i - 1];
		}
	}
	for (j = 0; j < 12; j++) {
		RsltUpDown3[j][0] = RsltUpDown[j][0];
	}
	for (j = 0; j < 12; j++) {
		k = 0;
		for (i = 0; i < NumColor[indx1]; i++) {
			if (RsltUpDown2[j][i] != 0) {
				k++;
				RsltUpDown3[j][k] = RsltUpDown3[j][k - 1] + RsltUpDown2[j][i];
				//				printf("%d %d\n",k,RsltUpDown3[j][k]);
			}
		}
	}
	for (i = 0; i < NN; i++) {
		fprintf(fp2, "%2d %2d %2d %2d %2d %2d %2d %2d %2d %2d %2d %2d %2d\n", i,
			RsltUpDown[0][i], RsltUpDown[1][i], RsltUpDown[2][i], RsltUpDown[3][i],
			RsltUpDown[4][i], RsltUpDown[5][i], RsltUpDown[6][i], RsltUpDown[7][i],
			RsltUpDown[8][i], RsltUpDown[9][i], RsltUpDown[10][i], RsltUpDown[11][i]);
	}
	for (j = 0; j < 12; j++) {
		cntr2 = 0;
		for (i = 0; i < NN - 2; i++) {
			if ((RsltUpDown3[j][i] + RsltUpDown3[j][i + 2]) != (RsltUpDown3[j][i + 1] * 2)) {
				if (RsltUpDown3[j][i + 2] != 0) {
					if (cntr > 2) {
						Sum[j] += RsltUpDown3[j][i + 1];
						ReversalPoints[j][cntr2] = RsltUpDown3[j][i + 1];
						cntr2++;
					}
					cntr++;
				}
			}
		}
		if ((cntr <= 3) && (cntr > 0)) {
			printf("No of reversal points in %d was <= 3.(cntr=%d)\n", j, cntr);
			Mean[j] = (double)Sum[j] / (cntr);
			if (Sum[j] != 0) printf("%d  %d  %lf\n", Sum[j], cntr, Mean[j]);

		}
		else {
			Mean[j] = (double)Sum[j] / (cntr - 3);
			if (Sum[j] != 0) printf("%d  %d  %lf\n", Sum[j], cntr - 3, Mean[j]);
		}
		cntr = 0;
	}
	double sum;
	double avg[12];
	for (i = 0; i < 12; i++) {
		sum = 0; cntr2 = 0; avg[i] = 0;
		while (ReversalPoints[i][cntr2] != 0) {
			sum += ReversalPoints[i][cntr2];
			cntr2++;
		}
		if (cntr2 > 0) {
			avg[i] = (double)sum / cntr2;
			//                        printf("avr[%d]/%d=%lf\n",i,cntr2,avr[i]);
		}
	}
	double sd[12];
	for (i = 0; i < 12; i++) {
		sum = 0; cntr2 = 0; sd[i] = 0;
		while (ReversalPoints[i][cntr2] != 0) {
			//printf("a %d sum=%lf\n",(int)ReversalPoints[i][cntr2],sum);
			//printf("a %d\n",(int)ReversalPoints[i][cntr2]);
			sum += pow((ReversalPoints[i][cntr2] - avg[i]), 2.0);
			cntr2++;
		}
		if (cntr2 > 1) {
			sd[i] = sqrt((double)sum / (cntr2 - 1));
			printf("%8.5lf %5.1lf avg:%lf sd:%8.5lf\n", sum, PhaseAngle[i], avg[i], sd[i]);
		}
	}
	for (Cntr = 0; Cntr < 12; Cntr++) {
		if (Sum[Cntr] != 0) {
			Correct79 = Mean[Cntr];
			/*		    printf("%f\n",Correct79);*/
			amp = 0.0005 * pow(1.258925, (double)Correct79);
			Red = amp * cos(2.0 * pi * PhaseAngle[Cntr] / 360);
			Green = amp * sin(2.0 * pi * PhaseAngle[Cntr] / 360);
			fprintf(fp3, "%lf %lf %lf %lf %lf %lf %lf\n",
				Red, Green, dummy, dummy, Correct79, dummy, amp);
		}
	}
	for (Cntr = 0; Cntr < 12; Cntr++) {
		if (Sum[Cntr] != 0) {
			Correct79 = Mean[Cntr];
			amp = 0.0005 * pow(1.258925, (double)Correct79);
			Red = amp * cos(2.0 * pi * PhaseAngle[Cntr] / 360);
			Green = amp * sin(2.0 * pi * PhaseAngle[Cntr] / 360);
			fprintf(fp3, "%lf %lf %lf %lf %lf %lf %lf\n",
				Red * -1.0, Green * -1.0, dummy, dummy, Correct79, dummy, amp);
		}
	}
	return 0;
}
// プログラムの実行: Ctrl + F5 または [デバッグ] > [デバッグなしで開始] メニュー
// プログラムのデバッグ: F5 または [デバッグ] > [デバッグの開始] メニュー

// 作業を開始するためのヒント: 
//    1. ソリューション エクスプローラー ウィンドウを使用してファイルを追加/管理します 
//   2. チーム エクスプローラー ウィンドウを使用してソース管理に接続します
//   3. 出力ウィンドウを使用して、ビルド出力とその他のメッセージを表示します
//   4. エラー一覧ウィンドウを使用してエラーを表示します
//   5. [プロジェクト] > [新しい項目の追加] と移動して新しいコード ファイルを作成するか、[プロジェクト] > [既存の項目の追加] と移動して既存のコード ファイルをプロジェクトに追加します
//   6. 後ほどこのプロジェクトを再び開く場合、[ファイル] > [開く] > [プロジェクト] と移動して .sln ファイルを選択します
