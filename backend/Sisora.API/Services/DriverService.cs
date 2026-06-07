using Microsoft.EntityFrameworkCore;
using Sisora.API.Data;
using Sisora.API.Helpers;
using Sisora.API.Models.DTOs.Requests;
using Sisora.API.Models.DTOs.Responses;
using Sisora.API.Models.Entities;
using Sisora.API.Models.Enums;
using Sisora.API.Services.Interfaces;

namespace Sisora.API.Services;

public class DriverService : IDriverService
{
    private readonly AppDbContext _context;

    public DriverService(AppDbContext context)
    {
        _context = context;
    }

    // ── Create Route ──────────────────────────────────────
    public async Task<ApiResponse<ServiceRouteResponse>> CreateRouteAsync(Guid driverId, CreateServiceRouteRequest request)
    {
        var driver = await _context.Drivers.FindAsync(driverId);

        if (driver == null)
            return ApiResponse<ServiceRouteResponse>.Fail("Driver not found.");

        if (driver.Status != DriverStatus.Approved)
            return ApiResponse<ServiceRouteResponse>.Fail("Your account must be approved before creating routes.");

        var route = new ServiceRoute
        {
            DriverId = driverId,
            Name = request.Name,
            AreaDescription = request.AreaDescription
        };

        await _context.ServiceRoutes.AddAsync(route);
        await _context.SaveChangesAsync();

        return ApiResponse<ServiceRouteResponse>.Ok(MapToRouteResponse(route, 0));
    }

    // ── Get My Routes ─────────────────────────────────────
    public async Task<ApiResponse<List<ServiceRouteResponse>>> GetMyRoutesAsync(Guid driverId)
    {
        var routes = await _context.ServiceRoutes
            .Where(r => r.DriverId == driverId && r.IsActive)
            .Include(r => r.Students)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        var response = routes
            .Select(r => MapToRouteResponse(r, r.Students.Count(s => s.IsActive)))
            .ToList();

        return ApiResponse<List<ServiceRouteResponse>>.Ok(response);
    }

    // ── Update Route ──────────────────────────────────────
    public async Task<ApiResponse<ServiceRouteResponse>> UpdateRouteAsync(Guid driverId, Guid routeId, UpdateServiceRouteRequest request)
    {
        var route = await _context.ServiceRoutes
            .Include(r => r.Students)
            .FirstOrDefaultAsync(r => r.Id == routeId && r.DriverId == driverId);

        if (route == null)
            return ApiResponse<ServiceRouteResponse>.Fail("Route not found.");

        route.Name = request.Name;
        route.AreaDescription = request.AreaDescription;

        await _context.SaveChangesAsync();

        return ApiResponse<ServiceRouteResponse>.Ok(
            MapToRouteResponse(route, route.Students.Count(s => s.IsActive)));
    }

    // ── Delete Route ──────────────────────────────────────
    public async Task<ApiResponse<bool>> DeleteRouteAsync(Guid driverId, Guid routeId)
    {
        var route = await _context.ServiceRoutes
            .FirstOrDefaultAsync(r => r.Id == routeId && r.DriverId == driverId);

        if (route == null)
            return ApiResponse<bool>.Fail("Route not found.");

        // soft delete
        route.IsActive = false;
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Route deleted successfully.");
    }

    // ── Add Student ───────────────────────────────────────
    public async Task<ApiResponse<StudentResponse>> AddStudentAsync(Guid driverId, Guid routeId, AddStudentRequest request)
    {
        var route = await _context.ServiceRoutes
            .FirstOrDefaultAsync(r => r.Id == routeId && r.DriverId == driverId && r.IsActive);

        if (route == null)
            return ApiResponse<StudentResponse>.Fail("Route not found.");

        // generate unique invite code
        string inviteCode;
        do
        {
            inviteCode = InviteCodeHelper.Generate();
        }
        while (await _context.Students.AnyAsync(s => s.InviteCode == inviteCode));

        var student = new Student
        {
            ServiceRouteId = routeId,
            FullName = request.FullName,
            SchoolName = request.SchoolName,
            PickupAddress = request.PickupAddress,
            InviteCode = inviteCode
        };

        await _context.Students.AddAsync(student);
        await _context.SaveChangesAsync();

        return ApiResponse<StudentResponse>.Ok(MapToStudentResponse(student));
    }

    // ── Get Students ──────────────────────────────────────
    public async Task<ApiResponse<List<StudentResponse>>> GetStudentsAsync(Guid driverId, Guid routeId)
    {
        var routeExists = await _context.ServiceRoutes
            .AnyAsync(r => r.Id == routeId && r.DriverId == driverId);

        if (!routeExists)
            return ApiResponse<List<StudentResponse>>.Fail("Route not found.");

        var students = await _context.Students
            .Where(s => s.ServiceRouteId == routeId && s.IsActive)
            .OrderBy(s => s.FullName)
            .ToListAsync();

        return ApiResponse<List<StudentResponse>>.Ok(
            students.Select(MapToStudentResponse).ToList());
    }

    // ── Update Student ────────────────────────────────────
    public async Task<ApiResponse<StudentResponse>> UpdateStudentAsync(Guid driverId, Guid routeId, Guid studentId, UpdateStudentRequest request)
    {
        var route = await _context.ServiceRoutes
            .FirstOrDefaultAsync(r => r.Id == routeId && r.DriverId == driverId);

        if (route == null)
            return ApiResponse<StudentResponse>.Fail("Route not found.");

        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.Id == studentId && s.ServiceRouteId == routeId);

        if (student == null)
            return ApiResponse<StudentResponse>.Fail("Student not found.");

        student.FullName = request.FullName;
        student.SchoolName = request.SchoolName;
        student.PickupAddress = request.PickupAddress;

        await _context.SaveChangesAsync();

        return ApiResponse<StudentResponse>.Ok(MapToStudentResponse(student));
    }

    // ── Remove Student ────────────────────────────────────
    public async Task<ApiResponse<bool>> RemoveStudentAsync(Guid driverId, Guid routeId, Guid studentId)
    {
        var route = await _context.ServiceRoutes
            .FirstOrDefaultAsync(r => r.Id == routeId && r.DriverId == driverId);

        if (route == null)
            return ApiResponse<bool>.Fail("Route not found.");

        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.Id == studentId && s.ServiceRouteId == routeId);

        if (student == null)
            return ApiResponse<bool>.Fail("Student not found.");

        // soft delete
        student.IsActive = false;
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Student removed successfully.");
    }

    // ── Regenerate Invite Code ────────────────────────────
    public async Task<ApiResponse<InviteCodeResponse>> RegenerateInviteCodeAsync(Guid driverId, Guid routeId, Guid studentId)
    {
        var route = await _context.ServiceRoutes
            .FirstOrDefaultAsync(r => r.Id == routeId && r.DriverId == driverId);

        if (route == null)
            return ApiResponse<InviteCodeResponse>.Fail("Route not found.");

        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.Id == studentId && s.ServiceRouteId == routeId);

        if (student == null)
            return ApiResponse<InviteCodeResponse>.Fail("Student not found.");

        // generate new unique code
        string inviteCode;
        do
        {
            inviteCode = InviteCodeHelper.Generate();
        }
        while (await _context.Students.AnyAsync(s => s.InviteCode == inviteCode));

        student.InviteCode = inviteCode;
        student.InviteCodeUsed = false;
        await _context.SaveChangesAsync();

        return ApiResponse<InviteCodeResponse>.Ok(BuildInviteCodeResponse(student));
    }

    // ── Private Mappers ───────────────────────────────────
    private static ServiceRouteResponse MapToRouteResponse(ServiceRoute route, int studentCount) => new()
    {
        Id = route.Id,
        Name = route.Name,
        AreaDescription = route.AreaDescription,
        IsActive = route.IsActive,
        StudentCount = studentCount,
        CreatedAt = route.CreatedAt
    };

    private static StudentResponse MapToStudentResponse(Student student) => new()
    {
        Id = student.Id,
        FullName = student.FullName,
        SchoolName = student.SchoolName,
        PickupAddress = student.PickupAddress,
        InviteCode = student.InviteCode,
        InviteCodeUsed = student.InviteCodeUsed,
        IsActive = student.IsActive,
        CreatedAt = student.CreatedAt
    };

    private static InviteCodeResponse BuildInviteCodeResponse(Student student) => new()
    {
        InviteCode = student.InviteCode,
        StudentName = student.FullName,
        WhatsAppMessage = $"Hi! Please download the Sisora app at sisora.lk and enter this code to track {student.FullName}: *{student.InviteCode}*"
    };
}