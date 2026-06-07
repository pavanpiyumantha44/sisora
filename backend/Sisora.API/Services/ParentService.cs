using Microsoft.EntityFrameworkCore;
using Sisora.API.Data;
using Sisora.API.Models.DTOs.Requests;
using Sisora.API.Models.DTOs.Responses;
using Sisora.API.Models.Entities;
using Sisora.API.Services.Interfaces;

namespace Sisora.API.Services;

public class ParentService : IParentService
{
    private readonly AppDbContext _context;

    public ParentService(AppDbContext context)
    {
        _context = context;
    }

    // ── Redeem Invite Code ────────────────────────────────
    public async Task<ApiResponse<StudentResponse>> RedeemInviteCodeAsync(Guid parentId, RedeemInviteCodeRequest request)
    {
        var code = request.InviteCode.Trim().ToUpper();

        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.InviteCode == code && s.IsActive);

        if (student == null)
            return ApiResponse<StudentResponse>.Fail("Invalid invite code.");

        if (student.InviteCodeUsed)
            return ApiResponse<StudentResponse>.Fail("This invite code has already been used.");

        // check parent not already linked to this student
        var alreadyLinked = await _context.ParentStudents
            .AnyAsync(ps => ps.ParentId == parentId && ps.StudentId == student.Id);

        if (alreadyLinked)
            return ApiResponse<StudentResponse>.Fail("You are already linked to this student.");

        var parentStudent = new ParentStudent
        {
            ParentId = parentId,
            StudentId = student.Id
        };

        student.InviteCodeUsed = true;

        await _context.ParentStudents.AddAsync(parentStudent);
        await _context.SaveChangesAsync();

        return ApiResponse<StudentResponse>.Ok(
            MapToStudentResponse(student), "Successfully linked to student.");
    }

    // ── Get My Children ───────────────────────────────────
    public async Task<ApiResponse<List<StudentResponse>>> GetMyChildrenAsync(Guid parentId)
    {
        var students = await _context.ParentStudents
            .Where(ps => ps.ParentId == parentId)
            .Include(ps => ps.Student)
            .Select(ps => ps.Student)
            .Where(s => s.IsActive)
            .OrderBy(s => s.FullName)
            .ToListAsync();

        return ApiResponse<List<StudentResponse>>.Ok(
            students.Select(MapToStudentResponse).ToList());
    }

    // ── Private Mapper ────────────────────────────────────
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
}