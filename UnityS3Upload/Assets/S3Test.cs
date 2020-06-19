using System.IO;
using UnityEngine;

using Amazon;
using Amazon.S3;
using Amazon.CognitoIdentity;
using Amazon.S3.Model;


public class S3Test : MonoBehaviour
{

    [Header("Credentials")]
    [SerializeField]
    private string identityPoolID = "us-east-2:00000000-0000-0000-0000-000000000000";
    [SerializeField]
    private string bucket = "BUCKET-NAME-HERE";

    [Header("File Info")]
    [SerializeField]
    private string fileName = "test.txt";

    private RegionEndpoint region = RegionEndpoint.USEast2;

    private string filePath;


    private CognitoAWSCredentials credentials;
    private AmazonS3Client s3Client;


    void Update()
    {
        if (Input.GetKeyDown("space")) {
            print("initing");
            InitS3();
            UploadFileAsync();
        }
    }

    void InitS3() {

        // Amazon examples don't include this line. Without it, this throws
        // "Exception: Main thread has not been set, is the AWSPrefab on the scene?".
        // According to the issue tracker, AWSPrefab is deprecated, and this is the recommended alternative.
        UnityInitializer.AttachToGameObject(gameObject);

        // Amazon examples don't include this line. Without it, this throws
        // "InvalidOperationException: Cannot override system-specified headers".
        AWSConfigs.HttpClient = AWSConfigs.HttpClientOption.UnityWebRequest;

        filePath = Path.GetFullPath(fileName);

        // Initialize the Amazon Cognito credentials provider
        credentials = new CognitoAWSCredentials(identityPoolID, RegionEndpoint.USEast2);

        // Amazon examples don't include second parameter. Without it, this throws
        // "AmazonClientException: No RegionEndpoint or ServiceURL configured".
        s3Client = new AmazonS3Client(credentials, region);
    }

    void UploadFileAsync() {

        Debug.Log("Retrieving file");

        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

        Debug.Log("Creating request object");

        var request = new PostObjectRequest() {
            Bucket = bucket,
            Key = fileName,
            InputStream = stream,
            CannedACL = S3CannedACL.Private,
            // Amazon examples don't include this line. Without it, PostObjectAsync fails silently.
            Region = region
        };

        Debug.Log("Making POST call");

        s3Client.PostObjectAsync(request, (responseObj) => {
            if (responseObj.Exception == null) {
                Debug.Log("success " + responseObj.Request.Key + " " + responseObj.Request.Bucket);
            } else {
                Debug.Log("error " + responseObj.Response.HttpStatusCode.ToString());
            }
        });

    }
}
