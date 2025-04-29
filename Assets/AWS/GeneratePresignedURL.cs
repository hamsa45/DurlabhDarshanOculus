using UnityEngine;
using TMPro;
using System;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

public class GeneratePresignedURL : MonoBehaviour
{
    //public TextMeshProUGUI urlText;

    void Start()
    {
        //const string bucketName = "co.techxrdev.durlabhdarshan";
        //const string objectKey = "DurlabhDarshanMobileApp/Caching/encryptedvideomp4.mp4";
        //const double timeoutDuration = 12;

        //AWSConfigsS3.UseSignatureVersion4 = true;
        //IAmazonS3 s3Client = new AmazonS3Client("AKIA4ALHXKCHDQ3LZSGA", "N5a016m5cygOgmrN5JDDaB+jWO1y8pLDfSQsHaca", Amazon.RegionEndpoint.APSouth1);

        //string urlString = GeneratePreSurl(s3Client, bucketName, objectKey, timeoutDuration);
        //Debug.Log("The generated URL is: " + urlString);

        //urlText.text = urlString;
    }

    public static string getVideoUrl(string bucketName, string objectKey, double duration)
    {
        //const string objectKey = "DurlabhDarshanMobileApp/Caching/encryptedvideomp4.mp4";
        double timeoutDuration = duration;

        AWSConfigsS3.UseSignatureVersion4 = true;
        IAmazonS3 s3Client = new AmazonS3Client("AKIA4ALHXKCHMXA2XIHH", "T1XgaiPmv+u62rGii+1aRrR+itPmR152bjB3Cllz", Amazon.RegionEndpoint.APSouth1);

        string urlString = GeneratePreSurl(s3Client, bucketName, objectKey, timeoutDuration);
        //Debug.Log("The generated URL is: " + urlString);

        //urlText.text = urlString;
        return urlString;
    }

    private static string GeneratePreSurl(IAmazonS3 client, string bucketName, string objectKey, double duration)
    {
        string urlString;

        try
        {
            var request = new GetPreSignedUrlRequest()
            {
                BucketName = bucketName,
                Key = objectKey,
                Expires = DateTime.UtcNow.AddHours(duration),
                Verb = HttpVerb.GET
            };

            urlString = client.GetPreSignedURL(request);
        }
        catch (AmazonS3Exception ex)
        {
            urlString = GeneratePreSurl(client, bucketName, objectKey, duration);
        }

        return urlString;
    }
}
