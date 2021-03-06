﻿//********************************************************* 
// 
// Copyright (c) Microsoft. All rights reserved. 
// This code is licensed under the MIT License (MIT). 
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF 
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY 
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR 
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT. 
// 
//********************************************************* 
namespace CognitiveAPIWrapper.SpeakerVerification
{
  public class VerificationEnrollmentResult
  {
    public VerificationEnrollmentStatus EnrollmentStatus { get; set; }
    public int EnrollmentsCount { get; set; }
    public int RemainingEnrollments { get; set; }
    public string Phrase { get; set; }
  }
}
