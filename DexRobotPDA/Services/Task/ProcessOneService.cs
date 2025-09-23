using System.Text.Json;
using Blazored.LocalStorage;
using Blazored.SessionStorage;
using DexRobotPDA.ApiResponses;
using DexRobotPDA.DTOs;
using RestSharp;

namespace DexRobotPDA.Services;

public class ProcessOneService : BaseService
{
    public ProcessOneService(
        RestClient restClient,
        ILogger<AuthService> logger, ILocalStorageService localStorage)
        : base(restClient, logger, localStorage)
    {
    }

    public async Task<List<MotorDto>?> GetFinishedList(string taskId)
    {
        var request = new RestRequest("api/Motor/GetFinishedList");
        request.AddParameter("taskId", taskId);
        return await ExecuteRequest<List<MotorDto>>(request);
    }

    public async Task<ApiResponse> AddMotor(AddMotorDto motorDto)
    {
        var request = new RestRequest("api/Motor/AddMotor", Method.Post);

        // 使用JSON格式发送电机数据
        request.AddJsonBody(motorDto);

        _logger.LogInformation("尝试新增电机 - 电机ID: {MotorId}", motorDto.motor_id);
        var apiResponse = await ExecuteCommand(request);

        // 序列化并打印响应
        var options = new JsonSerializerOptions { WriteIndented = true };
        string responseJson = JsonSerializer.Serialize(apiResponse, options);
        Console.WriteLine("API响应内容:");
        Console.WriteLine(responseJson);

        // 新增成功时处理返回数据并创建detect1记录
        if (apiResponse.ResultCode == 1 && apiResponse.ResultData != null)
        {
            var motorData = JsonSerializer.Serialize(apiResponse.ResultData);
            var createdMotor = JsonSerializer.Deserialize<MotorDto>(motorData);
            _logger.LogInformation("电机新增成功 - 电机ID: {MotorId}", createdMotor?.motor_id);

            // 创建检测记录DTO
            AddDetect1Dto detect1Dto = new AddDetect1Dto
            {
                motor_id = motorDto.motor_id,
                combine_time = DateTime.Now,
                remarks = motorDto.remarks ?? "电机创建时自动生成的检测记录",
                if_qualified = false
            };

            try
            {
                // 发送创建检测记录的请求
                var detectRequest = new RestRequest("api/Detect1/AddDetect1", Method.Post);
                detectRequest.AddJsonBody(detect1Dto);

                _logger.LogInformation("开始为电机 {MotorId} 创建检测记录", motorDto.motor_id);
                var detectResponse = await ExecuteCommand(detectRequest);

                if (detectResponse.ResultCode == 1)
                {
                    _logger.LogInformation("电机 {MotorId} 的检测记录创建成功", motorDto.motor_id);
                    // 如果需要，可以将检测记录信息添加到返回结果中
                    apiResponse.ResultData = new
                    {
                        motor = createdMotor,
                        detect1 = detectResponse.ResultData
                    };
                }
                else
                {
                    _logger.LogWarning("电机 {MotorId} 的检测记录创建失败: {Message}",
                        motorDto.motor_id, detectResponse.Msg);
                    // 检测记录创建失败不影响电机创建结果，但记录警告日志
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建电机 {MotorId} 的检测记录时发生异常", motorDto.motor_id);
                // 捕获异常但不影响主流程，确保电机创建成功的状态被正确返回
            }
        }
        else
        {
            _logger.LogWarning("电机新增失败 - 电机ID: {MotorId}, 错误信息: {Msg}",
                motorDto.motor_id, apiResponse.Msg);
        }

        return apiResponse;
    }
    
}