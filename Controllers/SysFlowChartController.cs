using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;

namespace PcbErpApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SysFlowChartController : ControllerBase
    {
        private readonly PcbErpContext _context;

        public SysFlowChartController(PcbErpContext context)
        {
            _context = context;
        }

        #region 節點位置

        /// <summary>
        /// 取得指定系統模組的節點位置（回傳 { nodeId: { x, y } } 字典）
        /// </summary>
        [HttpGet("{systemId}/positions")]
        public async Task<IActionResult> GetPositions(string systemId)
        {
            var nodes = await _context.SysFlowNodes
                .Where(n => n.SystemId == systemId)
                .ToListAsync();

            var result = nodes.ToDictionary(
                n => n.NodeId,
                n => new { x = n.X, y = n.Y }
            );

            return Ok(result);
        }

        /// <summary>
        /// 批次儲存節點位置（upsert）
        /// </summary>
        [HttpPut("{systemId}/positions")]
        public async Task<IActionResult> SavePositions(string systemId, [FromBody] List<FlowNodePositionRequest> positions)
        {
            if (positions == null || positions.Count == 0)
                return BadRequest(new { error = "positions 不可為空" });

            foreach (var pos in positions)
            {
                if (string.IsNullOrWhiteSpace(pos.NodeId)) continue;

                await _context.Database.ExecuteSqlRawAsync(
                    @"IF EXISTS (SELECT 1 FROM SysFlowNode WHERE SystemId = {0} AND NodeId = {1})
                          UPDATE SysFlowNode SET X = {2}, Y = {3} WHERE SystemId = {0} AND NodeId = {1}
                      ELSE
                          INSERT INTO SysFlowNode (SystemId, NodeId, X, Y) VALUES ({0}, {1}, {2}, {3})",
                    systemId, pos.NodeId, pos.X, pos.Y);
            }

            return Ok(new { message = "位置已儲存", count = positions.Count });
        }

        /// <summary>
        /// 刪除指定系統模組的所有節點位置（重設回預設）
        /// </summary>
        [HttpDelete("{systemId}/positions")]
        public async Task<IActionResult> ResetPositions(string systemId)
        {
            await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM SysFlowNode WHERE SystemId = {0}", systemId);

            return Ok(new { message = "位置已重設" });
        }

        #endregion

        #region 連線端口覆寫

        /// <summary>
        /// 取得指定系統模組的連線端口覆寫（回傳 { edgeKey: { fromPort, toPort } } 字典）
        /// </summary>
        [HttpGet("{systemId}/edges")]
        public async Task<IActionResult> GetEdges(string systemId)
        {
            var edges = await _context.SysFlowEdges
                .Where(e => e.SystemId == systemId)
                .ToListAsync();

            var result = edges.ToDictionary(
                e => e.EdgeKey,
                e => new { fromPort = e.FromPort, toPort = e.ToPort, pivotOffset = e.PivotOffset }
            );

            return Ok(result);
        }

        /// <summary>
        /// 批次儲存連線端口覆寫（upsert）
        /// </summary>
        [HttpPut("{systemId}/edges")]
        public async Task<IActionResult> SaveEdges(string systemId, [FromBody] List<FlowEdgeRequest> edges)
        {
            if (edges == null || edges.Count == 0)
                return BadRequest(new { error = "edges 不可為空" });

            foreach (var edge in edges)
            {
                if (string.IsNullOrWhiteSpace(edge.EdgeKey)) continue;

                await _context.Database.ExecuteSqlRawAsync(
                    @"IF EXISTS (SELECT 1 FROM SysFlowEdge WHERE SystemId = {0} AND EdgeKey = {1})
                          UPDATE SysFlowEdge SET FromPort = {2}, ToPort = {3}, PivotOffset = {4} WHERE SystemId = {0} AND EdgeKey = {1}
                      ELSE
                          INSERT INTO SysFlowEdge (SystemId, EdgeKey, FromPort, ToPort, PivotOffset) VALUES ({0}, {1}, {2}, {3}, {4})",
                    systemId, edge.EdgeKey, edge.FromPort, edge.ToPort, edge.PivotOffset);
            }

            return Ok(new { message = "連線端口已儲存", count = edges.Count });
        }

        /// <summary>
        /// 刪除指定系統模組的所有連線端口覆寫（重設回預設）
        /// </summary>
        [HttpDelete("{systemId}/edges")]
        public async Task<IActionResult> ResetEdges(string systemId)
        {
            await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM SysFlowEdge WHERE SystemId = {0}", systemId);

            return Ok(new { message = "連線端口已重設" });
        }

        #endregion
    }

    public class FlowNodePositionRequest
    {
        public string NodeId { get; set; } = string.Empty;
        public double X { get; set; }
        public double Y { get; set; }
    }

    public class FlowEdgeRequest
    {
        public string EdgeKey     { get; set; } = string.Empty;
        public string FromPort    { get; set; } = string.Empty;
        public string ToPort      { get; set; } = string.Empty;
        public double PivotOffset { get; set; }
    }
}
