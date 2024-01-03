using Microsoft.EntityFrameworkCore;
using SharedLibrary.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UdemyAuthServer.Core.Repositories;
using UdemyAuthServer.Core.Services;
using UdemyAuthServer.Core.UnitOfWork;

namespace UdemyAuthServer.Service.Services
{
    public class ServiceGeneric<TEntity, TDto> : IServiceGeneric<TEntity, TDto> where TEntity : class where TDto : class
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<TEntity> _genericRepository;

        public ServiceGeneric(IGenericRepository<TEntity> genericRepository, IUnitOfWork unitOfWork)
        {
            _genericRepository = genericRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<TDto>> AddAsync(TDto entity)
        {
            var newEntity = ObjectMapper.Mapper.Map<TEntity>(entity);
            await _genericRepository.AddAsync(newEntity);

            await _unitOfWork.CommitAsync();

            var newDto=ObjectMapper.Mapper.Map<TDto>(entity);
            return Response<TDto>.Success(newDto, (int)HttpStatusCode.OK);
        }

        public async Task<Response<IEnumerable<TDto>>> GetAllAsync()
        {
           var products = ObjectMapper.Mapper.Map<List<TDto>>(await _genericRepository.GetAllAsync());

            return Response<IEnumerable<TDto>>.Success(products, (int)HttpStatusCode.OK);
        }

        public async Task<Response<TDto>> GetByIdAsync(int id)
        {
            var product= await _genericRepository.GetByIdAsync(id);

            if (product == null)
            {
                return Response<TDto>.Fail("Id not found", (int)HttpStatusCode.NotFound, true);
            }

            return Response<TDto>.Success(ObjectMapper.Mapper.Map<TDto>(product), (int)HttpStatusCode.OK);
        }

        public async Task<Response<NoDataDto>> Remove(int id)
        {
            var isExist = await _genericRepository.GetByIdAsync(id);

            if (isExist == null)
            {
                return Response<NoDataDto>.Fail("Id not found", (int)HttpStatusCode.NotFound, true);
            }

            _genericRepository.Remove(isExist);

            await _unitOfWork.CommitAsync();

            return Response<NoDataDto>.Success((int)HttpStatusCode.NoContent);
        }

        public async Task<Response<NoDataDto>> Update(TDto entity, int id)
        {
            var isExist = _genericRepository.GetByIdAsync(id);

            if (isExist == null)
            {
                return Response<NoDataDto>.Fail("Id not found", (int)HttpStatusCode.NotFound, true);
            }

            var updateEntity = ObjectMapper.Mapper.Map<TEntity>(entity);
            _genericRepository.Update(updateEntity);
            await _unitOfWork.CommitAsync();

            return Response<NoDataDto>.Success((int)HttpStatusCode.NoContent);
        }

        public async Task<Response<IEnumerable<TDto>>> Where(Expression<Func<TEntity, bool>> predicate)
        {
            var products = _genericRepository.Where(predicate);
            return Response<IEnumerable<TDto>>.Success(ObjectMapper.Mapper.Map<IEnumerable<TDto>>( await products.ToListAsync()), (int)HttpStatusCode.OK);
        }
    }
}
